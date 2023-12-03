require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot
create filtered max-line allot

variable buff-len
variable filter-len

: make-string       ( addr len -- )
    dup dup create , here swap allot swap move
    does> dup cell+ swap @
;

s" one" make-string one
s" two" make-string two
s" three" make-string three
s" four" make-string four
s" five" make-string five
s" six" make-string six
s" seven" make-string seven
s" eight" make-string eight
s" nine" make-string nine

: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;

: ischar?       ( n -- f)
    dup 0 >= swap 9 <= and
;

: same?                 ( 'start len 'sub sub-len -- t|f )
    rot 2dup > if                   ( 'start 'sub sub-len len )     \ do we have enough chars for the number?
        2drop 2drop 0               ( 0 )                           \ no, so we can't match
    else
        drop                            ( 'start 'sub len )
        -1 swap 2swap rot               ( -1 'start 'sub len )               \ result - default true
        0 do                            ( f 'start 'sub )
            2dup                        ( f 'start 'sub 'start 'sub )
            i + c@ swap                 ( f 'start 'sub sub-c 'start' )
            i + c@ <> if                ( f 'start 'sub )
                2drop drop 0 0 0
                leave
            then                        ( f 'start 'sub )
        loop                            ( f 'start 'sub )
        2drop
    then 
;

: check-nums            ( start-pos -- n f )
    dup line-buffer +                           ( start-pos 'start )
    swap buff-len @ swap -                      ( 'start len )
    2dup one same? if 1 -1
    else 2dup two same? if 2 -1
    else 2dup three same? if  3 -1
    else 2dup four same? if 4 -1
    else 2dup five same? if 5 -1
    else 2dup six same? if 6 -1
    else 2dup seven same? if  7 -1
    else 2dup eight same? if 8 -1
    else 2dup nine same? if 9 -1
    else 0 0
    then then then then then then then then then    ( 'start len n f )
    2swap 2drop                                     ( n f )
;


: filter-line  ( -- )
    0 filter-len !
    buff-len @ 0 do
        i check-nums if
            filtered filter-len @ + c!
            1 filter-len +!
        else
            drop
            line-buffer i + c@ [char] 0 -
            dup ischar? if
                filtered filter-len @ + c!
                1 filter-len +!
            else drop then
        then
    loop
;

: calc-setting  ( -- n )
    filter-len 0 > if
        filtered c@ 10 *
        filtered filter-len @ 1- + c@ +
    else
        -1 abort" No digits in line"
    then
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