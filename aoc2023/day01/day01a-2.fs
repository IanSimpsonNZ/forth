require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" testa.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot
create filtered max-line allot

variable buff-len
variable filter-len

: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;

: ischar?       ( n -- f)
    dup 0 >= swap 9 <= and
;

: filter-line   ( -- )
    0 filter-len !
    buff-len @ 0 do
        line-buffer i + c@ [char] 0 -
        dup ischar? if
            filtered filter-len @ + c!
            1 filter-len +!
        else drop then
    loop
;

: calc-setting  ( -- n )
    filter-len 0 > if
\        ." first = " filtered c@ .
        filtered c@ 10 *
\        ." last = " filtered filter-len @ 1- + c@ .
        filtered filter-len @ 1- + c@ +
    else
        -1 abort" No digits in line"
    then

\    dup . cr
;

: process-line  ( -- n )
    buff-len @ 1 < if abort" Line too short" then
    filter-line
    calc-setting
;

: process       ( -- n )
    0                               \ accumulator
    begin get-line while
        buff-len !
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