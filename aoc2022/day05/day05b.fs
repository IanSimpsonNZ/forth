require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs

256 Constant max-line
Create line-buffer  max-line 2 + allot

9 constant max-stacks
64 constant max-stack-len

variable number-line
variable num-stacks

: array ( num-cells -- )
    create allot
    does> ( cell-num -- cell-ptr )
    swap cells +
;

max-stacks array stack-ptrs

: my-file1     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: my-file2     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: get-line      ( buff -- n-chr flag )
    line-buffer max-line fd-in read-line throw ;

: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )


: number-line?              ( line-count len -- line-count )
    dup 1 > if                                  ( line-count len )
        dup 1- line-buffer 1+ swap get-next-num ( line-count len n addr remaining )
        2drop 0> if                             ( line-count len )
            1+ 4 / dup max-stacks > if
                close-input -1 abort" Too many stacks" then   ( lc ns ) \ 3 chars for each column with space between columns aside from last column
            num-stacks !                        ( line-count )
            dup number-line !                   ( line-count )
        else drop then                          ( line-count )
    then
;

: find-number-of-stacks     ( -- )
    my-file1 open-input
    0 number-line !
    0                       \ line counter
    cr
    begin 1+ get-line while \ incr line counter and get line
        number-line?                            ( line-count )
        number-line @ 0= while                  ( line-count )
    repeat then
    drop
    close-input
    number-line @ 0= if -1 abort" Could not find stack numbers " then
;

: create-stacks             ( -- )
    num-stacks @ 0 do
        max-stack-len 1 cells q-create i stack-ptrs !
    loop
;

: process-stack-line        ( len -- )
    num-stacks @ dup                                    ( len #stacks #stacks )
    4 * 1- rot                                          ( #stacks expected-len len )
    - 0<> if -1 abort" Unexpected line length " then    ( #stacks )
    0 do                                                (  ) 
        i 4 * 1 + line-buffer + c@                      ( char )
        dup 32 <> if                                    ( char )
            dup i stack-ptrs @ q-push-front             ( char )
        then drop                                       (  )
    loop
;

: fill-stacks               ( -- )
    number-line @ 1 do
        get-line invert if -1 abort" Unexpected end of file reading stack data " then   ( len )
        process-stack-line

    loop
;

: .stacks                   ( -- )
    num-stacks @ 0 do
        i dup 1+ . ." : " stack-ptrs @          ( stack-addr )
        dup q-len 0 do                          ( stack addr )
            dup i q-data-ptr @ emit
        loop drop cr
    loop
;

: skip-to-moves             ( -- )
    2 0 do
        get-line
        invert if -1 abort" Unexpected end of file " then
        drop
    loop
;

: get-numbers               ( len -- #crates from to)
    line-buffer 5 + swap 5 - get-next-num       ( n1 addr rem )
    5 - swap 5 + swap get-next-num              ( n1 n2 addr rem )
    3 - swap 3 + swap get-next-num              ( n1 n2 n3 addr rem )
    2drop
;

: pick-em-up                ( #crates from to -- n1 n2 ... n #crates to )
    rot dup 2swap swap rot          ( #crates to from #crates )
    0 do                            ( #crates to from )     ( n1 #crates to from )      ( ... )
        dup stack-ptrs @ q-pop      ( #crates to from n1 )  ( n1 #crates to from n2)
        swap 2swap rot              ( n1 #crates to from )  ( n1 n2 #crates to from )
    loop drop                       ( n1 n2 ... n #crates to )
;

: put-em-down               ( n1 n2 ... n #crates to -- )
    swap 0 do                       ( n1 n2 ... n to )      ( n1 n2 ... n-1 to )
        dup rot swap                ( n1 n2 ... to n to )   ( n1 n2 ... to n-1 to )
        stack-ptrs @ q-push         ( n1 n2 ... to )        ( n1 n2 ... to )
    loop drop                       (  )
;

: do-moves                  ( -- )
    begin get-line while                ( len )
        get-numbers                     ( #crates from to)
        1- swap 1- swap                 \ move to zero base from 1 base
        2dup                            ( #crates from to from to )
        num-stacks @ >= if . -1 abort" Invalid to stack " then
        num-stacks @ >= if . -1 abort" Invalid from stack " then        ( #crates from to )
        pick-em-up
        put-em-down
    repeat
    drop
;

: process       ( -- )
    find-number-of-stacks
    create-stacks
    my-file2 open-input
    fill-stacks
    ." Start-> " cr
    .stacks
    skip-to-moves
    do-moves
    ." End-> " cr
    .stacks
    close-input
;

: .result       ( -- )
    ." The answer is: "
    num-stacks @ 0 do
        i stack-ptrs @ q-pop emit
    loop cr
;

: go
    process .result ;