require ~/forth/libs/files.fs

512 Constant max-line
Create line-buffer  max-line 2 + allot

: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;

: ischar?       ( n -- f)
    dup 0 >= swap 9 <= and
;

: get-first     ( num-chars -- n f )
    0 0                                             ( len acc f ) \ defaults - position 0, flag = not found
    rot 0 do                                        ( acc f )
        line-buffer i + c@ [char] 0 -               ( acc f n )
        dup ischar? if                              ( acc f n )
            rot drop swap drop -1                   ( n  -1 )
            leave
        then drop
    loop
;

: get-last      ( num-chars -- n f )
    0 0                                             ( len acc f ) \ defaults - position 0, flag = not found
    rot 1- 0 swap do                                ( acc f )
        line-buffer i + c@ [char] 0 -                ( acc f n )
        dup ischar? if                              ( acc f n )
            rot drop swap drop -1                   ( n  -1 )
            leave
        then drop
    -1 +loop       
;

: process-line  ( num-chars -- n )
    dup 2 < if abort" Line too short" then                                  ( len )
    dup get-first if 10 * else abort" Can't find first number" then      ( len len )
    swap get-last if + else abort" Can't find last number" then
;

: process       ( -- n )
    0                               \ accumulator
    cr begin get-line while
        process-line +
    repeat drop
;

: .result       ( n -- )
    cr ." The total is " . cr
;

: setup         ( -- )
;

: go
    s" input.txt" open-input
    setup
    process 
    close-input
    .result
;