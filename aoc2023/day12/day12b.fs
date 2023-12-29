require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/array.fs
require ~/forth/libs/strings.fs
require ~/forth/libs/structs.fs


: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

256 Constant max-line
Create line-buffer  max-line 2 + allot
variable buff-len

0 
1 cells field char-pos
1 cells field num-ptr
1 cells field orig-char-pos
max-line field placed-springs
constant #stack-rec


max-line 5 * $tring springs
max-line 5 * $tring springs-copy
max-line 5 * $tring springs-tmp
char . constant working
char # constant broken
char ? constant unknown
variable springs-len

128 constant max-q
create numbers max-q 1 cells q-create drop
create max-pos max-q 1 cells q-create drop

create old-recs 128 1 cells q-create q-init

max-line max-q * b-array cache-status
max-line max-q * b-array cache

0 constant cache-miss
1 constant cache-calculating
2 constant cache-hit

variable score
variable cumulative-score

variable numbers-len

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;

: cache-key        ( s-rec -- key )
    dup orig-char-pos @ swap num-ptr @                          ( char-pos num-ptr )
    dup 0< if -1 abort" cache-key: Negative num-ptr" then
    swap max-q * +                                              ( key )
;


: store-numbers     ( addr len -- )
    numbers q-init                                              ( addr len )
    begin $trim-front dup 0> while                              ( addr len )
        get-next-unsigned rot numbers q-push                    ( addr len )                   
    repeat 2drop                                                (  )
    numbers q-len numbers-len !
;

: store-springs     ( addr len -- )
    dup springs-len !
    springs $init                                               ( addr len )
    2dup springs drop swap move                                 ( addr len )
    springs drop 1 cells -                                      ( addr len springs-len-addr )
    ! drop                                                      (  )
;

: store-max-pos     ( -- )
    max-pos q-init                                              (  )
    springs swap drop                                           ( acc )
    0 numbers q-len 1- do                                       ( acc )
        numbers i q-data-ptr @                                  ( acc num )
        -                                                       ( acc )
        dup max-pos q-push-front                                ( acc )
        1-
    -1 +loop drop                                                   (  )
;

: .stack-rec            ( s-rec -- )
    dup placed-springs                                          ( s-rec addr)
    swap dup num-ptr @                                          ( addr s-rec num-ptr )
    swap char-pos @                                             ( addr num-ptr c-pos )
    ." c-pos: " . ." , num-ptr: " . ."  -> " springs-len @ type            (  )
;


: check-before      ( s-rec -- f )                              \ true if char before range is a valid terminator
    dup char-pos @ dup 0= if 2drop true else                    ( flag | s-rec c-pos )
    swap placed-springs + 1- c@                                 ( char-before )
    dup working = swap unknown = or                             ( flag )
    then
;

: check-after       ( s-rec -- flag )                           \ true if char after range is a valid terminator
    dup num-ptr @ over char-pos @                               ( s-rec num-ptr c-pos )
    swap numbers swap q-data-ptr @                              ( s-rec c-pos num )
    + dup                                                       ( s-rec end-pos end-pos )
    springs-len @ = if 2drop true else                          ( flag | s-rec end-pos )
    swap placed-springs + c@                                    ( char-after )
    dup working = swap unknown = or                             ( flag )
    then
;

: get-address-range     ( s-rec -- end-addr start-addr )        \ get range of addresses in springs-tmp
    dup num-ptr @ numbers swap q-data-ptr @                     ( s-rec num )
    swap char-pos @ springs-tmp drop +                          ( num cpos-addr )
    swap over +                                                 ( cpos-tmpaddr end-tmpaddr )
    swap                                                        ( end-tmpaddr cpos-tmpaddr )
;


: clear-left?           ( s-rec -- flag )                       \ are there no unallocated broken springs to our left
    dup char-pos @ dup 0> if                                    ( s-rec c-pos )
        swap placed-springs swap over + swap                    ( end-addr addr )
        true rot rot                                            ( flag end-addr addr )
        do                                                      ( flag )
            i c@ broken = if drop false leave then              ( flag )
        loop                                                    ( flag )
    else 2drop true then                                        ( flag )
;


: place?            ( s-rec -- s-rec' success? )                \ Can we place x number of broken springs here? Have already checked for enough space
    dup dup check-before swap check-after and                   ( s-rec )
    if                                                          ( s-rec )
        dup placed-springs springs-len @ springs-tmp $copy      ( s-rec )                     \ Make a copy of current string
        true over get-address-range                             ( s-rec success? end-addr start-addr)
        do                                                      ( s-rec success? )
            i c@ working = if drop false leave then             ( s-rec success? )
            [char] x i c!                                       ( s-rec success? )
        loop                                                    ( s-rec success? )
        dup if over clear-left? and then                        ( s-rec success? )
        dup if                                                  ( s-rec success? )              \ Copy marked string into stack record
            over placed-springs                                 ( s-rec success? str-addr )
            springs-tmp rot swap move                           ( s-rec success? )
        then                                                    ( s-rec success? )
    else                                                        ( s-rec )
        false                                                   ( s-rec success? )
    then                                                        ( s-rec success? )
;


: no-broken?        ( s-rec -- flag )
    true                                        ( s-rec flag )
    swap placed-springs                         ( flag str-addr )
    springs-len @ over + swap                   ( flag end-addr str-addr )
    do                                          ( flag )
        i c@ broken = invert and                ( flag )
    loop                                        ( flag )
;


: check-cache       ( s-rec -- score hit? )
    dup num-ptr @ numbers-len @ 1- = if drop 0 false else
        cache-key dup cache-status @                                    ( key status )
        cache-hit = if cache @ true else drop 0 false then              ( score hit? )
    then
;


: update-cache      ( s-rec -- )
    dup num-ptr @ numbers-len @ 1- < if
        cache-key dup cache-status @                                    ( key status )
        cache-calculating = if                                          ( key )
            dup dup cache @ score @ swap - swap cache !                 ( key )
            cache-status cache-hit swap !                               (  )
        else                                                            ( key )
            drop                                                        (  )
            ." Hmmmm. Not at 'calculating' in update-cache" cr
        then                                                            (  )
    else drop then
;


: incr-cpos             ( ... s-rec -- s-rec' )                         \ Get spring map from parent and move to next char position
    over placed-springs over placed-springs springs-len @ move          ( s-rec )
    dup dup char-pos @ 1+ swap char-pos !                               ( s-rec )
;


: create-stack-rec      ( c-pos num-ptr str-addr -- stack-rec-addr )
    old-recs q-len 0= if
        here #stack-rec allot                                           ( c-pos num-ptr str-addr rec-addr )
    else
        old-recs q-pop
    then
    swap over placed-springs springs-len @ move                         ( c-pos num-ptr rec-addr )
    swap over num-ptr !                                                 ( c-pos rec-addr )
    2dup char-pos !                                                     ( c-pos rec-addr )
    swap over orig-char-pos !                                           ( rec-addr )

    dup num-ptr @ dup 0>= swap numbers-len @ 1- < and if                ( rec-addr num-ptr>0 num-ptr )
        dup cache-key                                                   ( rec-addr key )                        \ check the cache
        dup cache-status @                                              ( rec-addr key status )
        dup cache-calculating = if ." Hmmmm. Found calculating status in create-stack-rec" cr then
        cache-miss = if                                                 ( rec-addr key )
            dup cache-status cache-calculating swap !                   ( rec-addr key )
            cache score @ swap !                                        ( rec-addr )
        else drop then                                                  ( rec-addr )
    then
;



: bump              ( ... stack-rec -- ... stack-rec' | ... stack-rec stack-rec-next' )            \ put next number on stack if there is one
    dup num-ptr @ numbers-len @ 1- = if                                 ( s-rec )
        dup no-broken? if 1 score +! then                               ( s-rec )
        incr-cpos                                                       ( s-rec )
    else                                                                ( s-rec )
        dup placed-springs                                              ( s-rec prev-str )
        over dup char-pos @                                             ( s-rec prev-str s-rec c-pos )
        swap num-ptr @                                                  ( s-rec prev-str c-pos num-ptr )
        dup numbers swap q-data-ptr @                                   ( s-rec prev-str c-pos num-ptr num )
        rot + swap 1+ rot                                               ( s-rec prev-str next-pos num-ptr+ prev-str )
        create-stack-rec                                                ( s-rec s-rec-next )
    then                                                                ( ... s-rec )
;

: still-room?           ( s-rec -- flag )
    dup char-pos @ swap num-ptr @                                       ( c-pos num-ptr )
    max-pos swap q-data-ptr @                                           ( c-pos max-pos )
    <=                                                                  ( flag )
;


: process-line      (  )
    springs dup 0= if abort" Could not find springs" else type ." : " then
    numbers dup q-len 0= if abort" Could not find numbers" else q. then
    -99 -99 springs drop create-stack-rec                               ( s-rec-base )           \ mark start of stack
    -99 -99 springs drop create-stack-rec                               ( s-rec-base )           \ Need two markers 'cos final incr-cpos reaches over to previous s-rec
    0 0 springs drop create-stack-rec                                   ( ... s-rec )
    begin dup num-ptr @ 0>= while                                       ( ... s-rec )
        dup check-cache if                                              ( ... s-rec score )
            score +!                                                    ( ,,, s-rec )
            old-recs q-push                                             ( ... prev-s-rec )
            incr-cpos                                                   ( ... prev-s-rec' )
        else                                                            ( ... s-rec score )
            drop                                                        ( ... s-rec )
            dup still-room? if                                          ( ... s-rec )
                place? if bump else incr-cpos then                      ( ... s-rec )           \ place? takes s-rec & modifies it, doesn't create new
                                                                                                \ else = doesn't fit here so move on one
            else                                                        ( ... s-rec )
                dup update-cache                                        ( ... s-rec )
                old-recs q-push                                         ( ... s-rec-prev )
                incr-cpos                                               ( ... s-rec-prev )
            then                                                        ( ... s-rec )
        then                                                            ( ... s-rec )
    repeat old-recs q-push old-recs q-push                              (  )
;


: pre-process       ( -- )                                      \ create 5 copies of springs and numbers
    springs springs-tmp $copy                                   (  )
    numbers q-len                                               ( len )
    4 0 do                                                      ( len )
        springs [char] ? $add-char 2drop                        ( len )
        springs springs-tmp $+                                  ( len )
        dup 0 do                                                ( len )
            numbers i q-data-ptr @ numbers q-push               ( len )
        loop                                                    ( len )
    loop drop                                                   (  )
    springs swap drop springs-len !
    numbers q-len numbers-len !
;

: process           ( -- )
    cr                                                          (  )
    begin get-line while                                        ( len )
        line-buffer swap 32 $split                              ( spring-addr spring-len numbers-addr numbers-len )
        1- swap 1+ swap                                         ( spring-addr spring-len numbers-addr numbers-len )  \ move past the space
        store-numbers                                           ( spring-addr spring-len )
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

