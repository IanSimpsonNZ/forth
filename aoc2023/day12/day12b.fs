require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/array.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot
variable buff-len

max-line 5 * $tring springs
max-line 5 * $tring springs-copy
char . constant working
char # constant broken
char ? constant unknown

128 constant max-q
create numbers max-q 1 cells q-create drop
create solution max-q 1 cells q-create drop
create max-pos max-q 1 cells q-create drop

max-line max-q * b-array cache-status
max-line max-q * b-array cache

0 constant cache-miss
1 constant cache-calculating
2 constant cache-hit

variable score
variable cumulative-score

variable first-broken

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;

: cache-key        ( c-pos num-ptr -- key )
    swap max-q * +
;


: store-numbers     ( addr len -- )
    numbers q-init                                              ( addr len )
    begin $trim-front dup 0> while                              ( addr len )
        get-next-unsigned rot numbers q-push                    ( addr len )                   
    repeat 2drop                                                (  )
;

: store-springs     ( addr len -- )
    springs $init                                               ( addr len )
    2dup springs drop swap move                                 ( addr len )
    springs drop 1 cells -                                      ( addr len springs-len-addr )
    ! drop                                                      (  )

    springs swap drop dup first-broken !                        ( len )                                 \ if no hashes, default to end of string
    0 do                                                        (  )
        springs drop i + c@                                     ( c )
        broken = if i first-broken ! leave then                 (  )
    loop                                                        (  )
;

: store-max-pos     ( -- )
    max-pos q-init                                              (  )
    springs swap drop                                           ( acc )
    0 numbers q-len 1- do                                       ( acc )
        numbers i q-data-ptr @                                 ( acc num )
        -                                                     ( acc )
        dup max-pos q-push-front                                ( acc )
        1-
    -1 +loop drop                                                   (  )
\    max-pos q-pop-front 1+ max-pos q-push-front 
;

: place?            ( c-pos num-ptr -- c-pos' success? )        \ Can we place x number of broken springs here?
    2dup 0= swap first-broken > and if drop false else          ( c-pos num-ptr | c-pos false )
    2dup max-pos swap q-data-ptr @ > if drop false else         ( c-pos num-ptr  )
    2dup numbers swap q-data-ptr @ dup rot +                    ( c-pos num-ptr num end-spring )
    springs rot < if 2drop drop false else                      ( c-pos false | c-pos num-ptr num addr )             \ Do we have room for these springs
        2over drop dup 2over swap drop +                        ( c-pos num-ptr num addr c-pos c-addr )
        1- c@ dup working = swap unknown = or over 0> and       ( c-pos num-ptr num addr c-pos f )
        swap 0= or if                                           ( c-pos num-ptr num addr c-pos )                     \ start of line or prev char is terminator?
            2over drop + swap over + dup rot                    ( c-pos num-ptr addr-end addr-end addr-start )
            true rot rot                                        ( c-pos num-ptr addr-end success? addr-end addr-start )
            do                                                  ( c-pos num-ptr addr-end success? )
                i c@ working = if drop false leave then         ( c-pos num-ptr addr-end success? )
            loop                                                ( c-pos num-ptr addr-end success? )
            if                                                  ( c-pos num-ptr addr-end )
                dup springs + = if                              ( c-pos num-ptr addr-end )                          \ end of springs
                    2drop drop springs swap drop true           ( c-pos true )
                else dup c@ dup working = swap unknown = or if  ( c-pos num-ptr addr-end+ )                         \ found a working spring
                    springs drop - true 2swap 2drop             ( c-pos' true )
                else                                            ( c-pos num-ptr addr-end+ )
                    2drop false                                 ( c-pos false )                                     \ neither
                then then                                       ( c-pos success? )
            else 2drop false then                               ( c-pos false )
        else 2drop drop false then                              ( c-pos false )
    then then then                                                      ( c-pos false )
;

: save-stack        ( list of c-pos num-ptr pairs -- )
    0 numbers q-len 1- do
        i <> if -1 abort" Unexpected num-ptr on stack" then     ( c-pos1 num-ptr1 c-pos2 num-ptr2 c-pos3 )
        solution q-push                                         ( c-pos1 num-ptr1 c-pos2 num-ptr2 )
    -1 +loop
;

: show-springs      (  )
    numbers q-len 0 do
        springs-copy drop                                       ( addr )
        solution dup q-len i - 1- q-data-ptr @ +                ( addr-start )
        dup numbers i q-data-ptr @ + swap                       ( addr-end addr-start )
        do [char] s i c! loop 
    loop
;

: restore-stack     ( -- list of c-pos num-ptr pairs )
    numbers q-len 0 do
        solution q-pop i
    loop
;

: no-broken?        ( -- flag )
    springs springs-copy $copy                  (  )
    solution q-init                             (  )
   
    save-stack                                  (  )
    show-springs                                (  )
    restore-stack                               (  )

    springs-copy type cr
\    over true swap 
\    springs-copy drop dup rot + swap
    true                                        ( true )
    springs-copy over + swap
    do
        i c@ broken = invert and                ( flag )
    loop
;



: check-cache       ( c-pos num-ptr -- score hit? )
\    2dup swap . . ." = "
    dup numbers q-len 1- = if                                       ( c-pos num-ptr )
        ." last item, no chache "
        2drop 0 false                                               ( score hit? )
    else
        cache-key dup cache-status @                                ( key status )
        dup cache-miss = if                                         ( key status )
            drop dup cache-status cache-calculating swap !          ( key )
            cache score @ swap !                                    (  )
            0 false                                                 ( score hit? )
        else cache-hit = if                                         ( key )
            cache @ true                                            ( score hit? )
        else                                                        ( key )
            -1 abort" Found 'calculating' status in check-cache"
        then then                                                   ( score hit? )
    then                                                            ( score hit? )
\    2dup swap . . cr
;


: update-cache      ( c-pos num-ptr -- )
    dup dup -99 > swap numbers q-len 1- < and if                                                    ( key status )
        cache-key dup cache-status @                                ( key status )
        cache-calculating = if                                      ( key )
            dup cache-status cache-hit swap !                       ( key )
            dup cache @ score @ swap -                                ( key score-delta )
            swap cache !                                            (  )
        else -1 abort" Not at 'calculating' status in update-cache"
        then
    else 2drop then
;

: bump              ( prev-c-pos prev-num-ptr c-pos num-ptr -- c-pos' num-ptr' )            \ either uses new c-pos and leaves old on stack, or updates old
    dup numbers q-len 1- = if                                   ( c-prev num-prev c-pos num-ptr  )
        2drop
        no-broken? if 1 score +! then                           ( c-prev num-prev )
        2dup update-cache
        swap 1+ swap                                            ( c-prev' num-prev )
        ."  S+ "
    else 1+ swap 1+ swap then                                   ( c-prev num-prev c-pos' num-ptr' ) \ leave 1 space for termination and then leave next num on stack
;



: process-line      (  )
    springs dup 0= if abort" Could not find springs" else type ." : " then
    numbers dup q-len 0= if abort" Could not find numbers" else q. cr then
    -99 -99                                                     ( -99 -99 )                                 \ mark start of stack
    0 0                                                         ( c-pos num-ptr )
    begin dup -99 > while                                       ( c-pos num-ptr )
        .s ." -> "
        2dup check-cache if                                     ( c-pos num-ptr score )
            ." Hit cache = " dup .
            score +!                                 ( c-pos num-ptr )
            swap 1+ swap                                        ( c-pos num-ptr )
        else                                                    ( c-pos num-ptr score )
            ." Miss "
            drop                                                ( c-pos num-ptr )
            2dup place?                                             ( c-pos num-ptr c-pos' success? )           \ c-pos' points to char after broken springs - . or ?
            if over bump else drop swap 1+ swap then                ( c-pos num-ptr c-pos' num-ptr' | c-pos' num-ptr )  \ else = doesn't fit here so move on one
            2dup max-pos swap q-data-ptr @ > if                 ( c-pos num-ptr c-pos' num-ptr' )
                2drop                                           ( c-pos num-ptr )
                2dup update-cache                               ( c-pos num-ptr )
                swap 1+ swap                                    ( c-pos num-ptr )
            then                                                ( c-pos num-ptr )
\            over springs swap drop >= if 2drop ( cache update ) swap 1+ swap then    ( c-pos num-ptr )                           \ if we reached the end move back up stack and increment that positiomn
        then                                                    ( c-pos num-ptr )
        cr
    repeat 2drop                                                (  )
;

: pre-process       ( -- )                                      \ create 5 copies of springs and numbers
    springs springs-copy $copy                                  (  )
    numbers q-len                                               ( len )
    4 0 do                                                      ( len )
        springs [char] ? $add-char 2drop                        ( len )
        springs springs-copy $+                                 ( len )
        dup 0 do                                                ( len )
            numbers i q-data-ptr @ numbers q-push               ( len )
        loop                                                    ( len )
    loop drop                                                   (  )
;

: process           ( -- )
    cr                                                          (  )
    begin get-line while                                        ( len )
        line-buffer swap 32 $split                              ( spring-addr sprint-len numbers-addr numbers-len )
        1- swap 1+ swap                                         ( spring-addr sprint-len numbers-addr numbers-len )  \ move past the space
        store-numbers                                           ( spring-addr sprint-len )
        store-springs                                           (  )
        pre-process
        store-max-pos                                           (  )
        0 cache b-array-init
        0 cache-status b-array-init
        0 score !
        process-line                                            (  )
        ."   Score = " score ? cr
        score @ cumulative-score +!
    repeat drop
;

: .result       ( n -- )
    cr ." The answer is " cumulative-score ? cr
;

: setup         ( -- )
    0 cumulative-score !
;

: go
    open-input
    setup
    process 
    close-input
    .result
;