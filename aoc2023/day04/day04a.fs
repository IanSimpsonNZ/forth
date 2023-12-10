require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot

variable buff-len

512 constant max-q-len 
create winning-nums max-q-len 1 cells q-create drop

: get-line      ( buff-addr -- num-chars flag ) max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )


: $split        ( addr len char -- addr1 len1 addr2 len2 )  \ find delim (char) in string - str1 ends before delim, str2 includes delim
                                                            \ str2 is empty (ie len 0) if delim not found in original string
    rot rot over + over                                 ( char addr end-addr c-addr )
    2swap swap 2swap                                    ( addr char end-addr c-addr )
    begin                                               ( addr char end-addr c-addr )
        2dup >                                          ( addr char end-addr c-addr f )
        2over drop rot dup c@ rot <> rot                ( addr char end-addr c-addr f f )
    and while                                           ( addr char end-addr c-addr )
        1+                                              ( addr char end-addr c-addr )
    repeat                                              ( addr char end-addr c-addr )
    dup rot swap -                                      ( addr char c-addr len2 )
    2swap drop rot 2dup swap -                          ( len2 addr c-addr len1 )
    rot swap 2swap swap                                 ( addr len1 c-addr len2 )
;

: $trim-front   ( addr len -- addr len )                    \ remove leading spaces
    begin 2dup 0> swap c@ 32 = and while                ( addr len )
    1- swap 1+ swap repeat                              ( addr+1 len-1 )
;


: get-winning-nums      ( addr len -- )
    winning-nums q-init
    $trim-front
    begin dup 0> while                              ( addr len )
        $trim-front
        get-next-num rot                            ( addr rem n )
        winning-nums q-push                         ( addr rem )
    repeat
    2drop
;

: match?                ( n -- f )
    false swap                                          ( f n )
    winning-nums q-len 0 do                             ( f n )
        dup winning-nums i q-data-ptr @ = if            ( f n )
            swap drop true swap leave                   ( f n )
        then
    loop drop
;

: check-nums            ( addr len -- n )
    0 rot rot                                           ( #matches addr len )
    $trim-front                                         ( #m addr len )
    begin dup 0> while                                  ( #m addr len )
        $trim-front
        get-next-num 2swap                              ( addr len #m n )
        match? if 1+ then                               ( addr len #m )
        rot rot                                         ( #m addr len )
    repeat                                              ( #m addr len )
    2drop dup 0> if 1- 1 swap lshift then               ( score )
;


: process-line  ( -- line-score )
    line-buffer buff-len @ type ."  w> "
    0                                                   ( acc )
    line-buffer buff-len @ [char] : $split              ( acc addr1 len1 addr2 len2 )
    2swap 2drop                                         ( acc addr2 len2 )
    swap 1+ swap [char] | $split                        ( acc addr1 len1 addr2 len2)
    2swap get-winning-nums                              ( acc addr2 len2 )
    winning-nums q.
    check-nums dup ." = " .
    +                                        ( acc )
    cr
;

: process       ( -- n )
    0                                                   ( acc )           \ accumulator
    cr                                                  ( acc )
    begin line-buffer get-line while                                ( acc len )
            buff-len !                                  ( acc )
            process-line +                              ( acc )
        repeat drop                                     ( acc )
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