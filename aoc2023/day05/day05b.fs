require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: array ( num-elements -- ) create cells allot
    does> ( element-number -- addr ) swap cells + ;


512 Constant max-line
Create line-buffer  max-line 2 + allot

variable buff-len


512 constant max-q-len
2 array queues
variable source-select

: sources       ( -- source-q-addr )
    source-select @ queues @
;

: destinations  ( -- dest-q-addr )
    source-select @ 1+ 2 mod queues @ 
;

: switch-source ( -- )
    source-select @ 1+ 2 mod source-select !
;

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )

: 3dup          ( x a b c -- x a b c a b c )
    2over swap 2over rot drop 
;

: range-push    ( start range q-addr -- )
    dup                                                 ( start range addr addr )
    q-top-ptr 2swap swap rot dup rot swap !             ( addr range t-addr )           \ Store values at top of stack
    1 cells + !                                         ( addr )
    q-top-incr                                          (  )                            \ increment top pos (with wraparound) and store
;

: q-get-double  ( addr n -- d1 d2 )
    q-data-ptr dup @ swap 1 cells + @
;

: range-q.            ( queue-addr -- )
    dup dup q-base-pos @ swap q-top-pos @ = if ." Empty " drop else
        dup q-len 0 do
            dup i q-data-ptr dup ." [" ? ."  , " 1 cells + ? ." ] " 
            loop
        drop
    then
;


: get-seeds     ( -- )
    get-line invert if -1 abort" Can't read from file" then         ( len )
    line-buffer swap [char] : $split                                ( addr1 len1 addr2 len2 )
    2swap 2drop                                                     ( addr2 len2 )
    1- swap 1+ swap $trim-front                                     ( addr len )
    begin dup 0> while                                              ( addr len )
        $trim-front
        get-next-num $trim-front get-next-num                       ( seed range addr rem )
        2swap sources range-push                                    ( addr rem )
    repeat
    2drop

    get-line
    invert if -1 abort" No maps" then
    0> if -1 abort" Expected a blank line after the seed data" then
;

: get-ranges            ( len -- dest source range )
    line-buffer swap
    $trim-front get-next-num                    ( dest addr rem )
    $trim-front get-next-num                    ( dest source addr rem )
    $trim-front get-next-num                    ( dest source range addr rem )
    2drop                                       ( dest source range )
;


: calc-overlap      ( dest-incr map-start map-end this-start this-range -- left-start left-range dest-start dest-range right-start right-range )
    over +                                                  ( dest-incr map-start map-end this-start this-end )
    2swap 2over rot min rot rot max                         ( dest-incr this-start this-end overlap-end overlap-start )
    2dup <= if                                              ( dest-incr this-start this-end overlap-end overlap-start )
        2drop rot drop                                      ( this-start this-end )
        over - over 0 over 0                                ( this-start this-range this-start 0 this-start 0 )
    else                                                    ( dest-incr this-start this-end overlap-end overlap-start )
        dup 2rot rot over -                                 ( this-end overlap-end overlap-start dest-incr this-start left-range )
                                                            ( this-start left-range this-end overlap-end overlap-start dest-incr overlap-end overlap-start dest-incr )
        2rot 2rot dup 2over rot + rot rot + over -          ( this-start left-range this-end overlap-end overlap-start dest-start dest-range )
        dup 2rot swap over -                                ( this-start left-range overlap-start dest-start dest-range d-r overlap-end right-range )
        2rot 2rot drop rot drop 2swap                       ( this-start left-range dest-start dest-range overlap-end right-range )
    then
;


: trans             ( dest source range -- )
    over + swap rot over - swap rot                         ( dest-incr source-start source-end )
    sources q-len 0 do                                      ( dest-incr source-start source-end )
        3dup sources i q-get-double                         ( dest-incr source-start source-end ith-start ith-range )
        calc-overlap                                        ( dest-incr source-start source-end left-start left-range dest-start dest-range right-start right-range )
        dup 0> if sources range-push else 2drop then        ( dest-incr source-start source-end left-start left-range dest-start dest-range )
        dup 0> if                                           ( dest-incr source-start source-end left-start left-range dest-start dest-range )
            destinations range-push                         ( dest-incr source-start source-end left-start left-range )
            sources i q-data-ptr 1 cells + ! drop           ( dest-incr source-start source-end )
        else 2drop 2drop then                               ( dest-incr source-start source-end )
    loop
    2drop drop
;


: process-map-lines      ( -- )
    begin get-line over 0> and while            ( len )
        get-ranges                              ( dest source length )
        trans                                   (  )
    repeat drop
;

: copy-remaining        ( -- )
    sources q-len 0 do                                  (  )
        sources i q-get-double dup                      ( s-start s-range s-range )
        0 > if destinations range-push else 2drop then  (  )
    loop
;

: process-map   ( -- f )
    get-line dup if                             ( len f )
        swap 0> if                              ( f )
            process-map-lines                   ( f )
            copy-remaining
        else -1 abort" Expected map title line" then
    else swap drop then                         ( f )
;


: process       ( -- )
    cr
    get-seeds
    begin
\        sources range-q. ." => "
        destinations q-init
        process-map
\        destinations range-q. cr
    while 
        switch-source
    repeat 
;

: .result       ( -- )
    sources q-len 0= if -1 abort" No data" then
    sources 0 q-data-ptr @                               ( n )
    sources q-len 0 do sources i q-data-ptr @ min loop    ( n )

    cr ." The minimum is " . cr
;

: setup         ( -- )
    max-q-len 2 cells q-create 0 queues !
    max-q-len 2 cells q-create 1 queues !

    0 source-select !

    sources q-init
    destinations q-init
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
