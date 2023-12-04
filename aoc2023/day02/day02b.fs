require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot

variable buff-len

variable max-red
variable max-green
variable max-blue

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

: red          ( n -- )
    max-red @ max max-red !
;

: green         ( n -- )
    max-green @ max max-green !
;

: blue          ( n -- )
    max-blue @ max max-blue !
;

: check-set     ( addr-start addr-end -- addr-end )
    begin                                           ( start end )
        2dup get-cubes                              ( start end delim found?)
    while                                           ( start end delim )
        rot 2dup -                                  ( end delim start len)
        ." Cube= " 2dup type cr
        evaluate                                    ( end delim )
        1+ swap                                     ( delim+1 end )
    repeat
    rot drop swap drop                              ( delim )      
;

: check-games   ( addr -- n )
    0 max-red !
    0 max-green !
    0 max-blue !
    begin                                           ( addr )
        dup get-set                                 ( addr addr-end found)
    while                                           ( addr addr-end )
        ." Set= " 2dup over - type cr
        check-set                                   ( addr-end )
        1 +                                         ( addr-next )
    repeat 2drop
    ." Max Red= " max-red ? ."  Max Green= " max-green ? ."  Max Blue= " max-blue ? 
    max-red @ max-green @ max-blue @ * *  ."  =>  " dup . cr
;

: process-line  ( -- n )                          \ Game id and valid game flag
    buff-len @ 8 < if abort" Line too short" then
    get-id                                          ( n addr)
    ." ID= " swap . cr                              ( addr )
    check-games                                     ( n )
;

: process       ( -- n )
    0                               \ accumulator
    cr
    begin get-line while
        buff-len !
        line-buffer buff-len @ type cr
        process-line + 
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