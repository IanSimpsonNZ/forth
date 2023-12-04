require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot

variable buff-len

12 constant max-red
13 constant max-green
14 constant max-blue

: get-line      ( -- num-chars flag )       line-buffer max-line fd-in read-line throw ;
: get-num       ( num-chars -- n addr )      0. rot line-buffer swap >number rot 2drop ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )

: get-id        ( -- n addr)
    line-buffer 5 + 4 get-next-num drop
;

: sub-str       ( addr-s addr-end c -- addr-found f )
    swap -1 2swap 2over drop rot                    ( addr-end true c addr-end addr-s )      \ set not default return results - start to eol
    2dup <= if                                      ( end true c eol start )
        2drop 2drop 0                               ( end false )               \ exit if search past eol
    else                                            ( end true c end start )
        do                                          ( end true c )
            dup i c@ = if                           ( end true c )
                rot drop i                          ( true c i )
                rot rot                             ( i true c )
                leave                               ( i true c )                \ exit loop if found
            then
        loop drop                                   ( addr f )
    then                                            ( addr f )
;

: get-set       ( addr-start -- addr-end f )
    line-buffer buff-len @ +                    ( addr-start eol )
    [char] ; sub-str                            ( addr-f f )
;

: get-cubes     ( set-start set-end -- addr-delim f )
    [char] , sub-str                            ( addr-f f )
;

: red          ( n -- f )
    max-red <=
;

: green         ( n -- f )
    max-green <=
;

: blue          ( n -- f )
    max-blue <=
;

: check-set     ( addr-start addr-end -- addr-end f )
    -1 over swap 2over                              ( start end end -1 start end )
    begin                                           ( start end end f start end )
        get-cubes 2over swap drop and               ( start end end f delim found&f )
    while                                           ( start end end f delim )
        dup 2rot drop dup rot swap -                ( end f delim start len )
        ." Cube= " 2dup type
        evaluate                                    ( end f delim f' )
        dup if ."  Ok " else ."  Invalid " then cr
        rot and                                     ( end delim f' )
        rot rot 1+                                  ( f' end delim+1 )
        over 2swap swap 2over                       ( delim+1 end end f' delim+1 end )
    repeat                                          ( start end end f delim )
    rot drop 2swap 2drop swap                       ( delim f )
;

: check-games   ( addr -- f )
    -1 swap                                         ( f addr )
    begin                                           ( f addr )
        2dup get-set rot and                        ( f addr addr-end found&f )
    while                                           ( f addr addr-end )
        ." Set= " 2dup over - type cr
        check-set                                   ( f addr-end f' )
        rot and swap over dup                       ( f' addr-end f' f')
    while                                           ( f' addr-end f' )
        drop                                        ( f' addr-end )
        1 +                                         ( f' addr-next )
    repeat then 2drop
;

: process-line  ( -- n f )                          \ Game id and valid game flag
    buff-len @ 8 < if abort" Line too short" then
    get-id                                          ( n addr)
    ." ID= " over . cr
    check-games                                     ( n f )
;

: process       ( -- n )
    0                               \ accumulator
    cr
    begin get-line while
        buff-len !
        line-buffer buff-len @ type cr
        process-line if + else drop then
    repeat drop
;

: .result       ( n -- )
    cr ." The total is " . cr
;

: setup         ( -- )
;

: go
    open-input
    setup
    process 
    close-input
    .result
;