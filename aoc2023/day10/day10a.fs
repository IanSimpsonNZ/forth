\ Have queue of queues
\ Start with input
\ add each queue of differences until each diff is the same
\ add back up the last element of ther queues

require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


256 constant max-y
create pipes max-y 1 cells q-create drop

512 Constant max-line
Create line-buffer  max-line 2 + allot
variable buff-len

\ variable x
\ variable y

124 constant n-s                \ |
45  constant e-w                \ -
76  constant n-e                \ L
74  constant n-w                \ J
55  constant s-w                \ 7
70  constant s-e                \ F


: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;

: store-line        ( -- )
    here buff-len @ allot                                       ( addr )
    dup pipes q-push                                            ( addr )
    line-buffer swap buff-len @ move                            (  )

;

: print-pipes       ( -- )
    pipes q-len 0 do
        pipes i q-data-ptr @ buff-len @ type cr
    loop
;

: findS             ( -- x y )                      \ Store start position in x and y
    -1 -1                                           ( x y )
    pipes q-len 0 do                                ( x y  )
        pipes i q-data-ptr @                        ( x y addr )
        buff-len @ 0 do                             ( x y addr )
            dup c@ [char] S = if                    ( x y addr )
                rot drop i rot drop j rot           ( x' y' addr )
            then                                    ( x y addr )
            1+                                      ( x y addr )
        loop                                        ( x y addr )
        drop                                        ( x y )
        dup 0>= if leave then                       ( x y )
    loop                                            ( x y )
    dup 0< if -1 abort" Couldn't find S" then       ( x y )
;

: get-c-addr        ( x y -- addr )
    pipes swap q-data-ptr @ +
;

: get-seg           ( x y -- c )
    get-c-addr c@
;

: check-up          ( x y -- f )                    \ true if is a connection to north of x y
    1- dup 0>= if                                   ( x y-1 )
        get-seg                                     ( c )
        dup n-s = swap                               ( f c )
        dup s-e = swap                               ( f f c )
        s-w = or or                                  ( f )
    else 2drop false then                           ( f )
;

: check-down        ( x y -- f )                    \ true if is a connection to north of x y
    1+ dup pipes q-len < if                         ( x y+1 )
        get-seg                                     ( c )
        dup n-s = swap                               ( f c )
        dup n-e = swap                               ( f f c )
        n-w = or or                                  ( f )
    else 2drop false then                           ( f )
;

: check-left          ( x y -- f )                  \ true if is a connection to north of x y
    swap 1- dup 0> if                               ( y x-1 )
        swap get-seg                                ( c )
        dup e-w = swap                               ( f c )
        dup s-e = swap                               ( f f c )
        n-e = or or                                  ( f )
    else 2drop false then                           ( f )
;

: check-right          ( x y -- f )                    \ true if is a connection to north of x y
    swap 1+ dup buff-len @ < if                       ( y x+1 )
        swap get-seg                                ( c )
        ." Checking >" dup emit cr
        dup e-w = swap                               ( f c )
        dup s-w = swap                               ( f f c )
        n-w = or or                                  ( f )
    else 2drop false then                           ( f )
;


: find-connection   ( x y -- x' y' )
    2dup check-up if 0 -1 else                      ( x y | x y 0 -1 )
    2dup check-down if 0 1 else                     ( x y | x y 0 1 )
    2dup check-left if -1 0 else                    ( x y | x y -1 0 )
    2dup check-right if 1 0 else                    ( x y | x y 1 0 )
    -1 abort" Can't find a connection"
    then then then then                             ( x y dx dy )
    rot + swap rot + swap                           ( y+dy x+dx )
;

: in-out            ( from-x from-y x y -- x y next-x next-y)                       \ given way in and symbol there is only one way out
    2over 2over 2swap swap ." From (" . ." , " . ." ) to (" swap . ." , " . ." )"

    2swap 2over rot - swap rot - swap                           ( x y dx dy )
    2dup 0<> swap 0<> and if -1 abort" Invalid deltas, both non-zero" then
    2dup abs swap abs max 1 > if -1 abort" Invalid delta, > 1" then

    2dup ."     Delta = (" swap . ." , " . ." )" cr

    2over get-seg                                               ( x y dx dy seg )
    dup n-s = if drop else                                      ( x y dx dy )                   \ keep moving in vertical direction - up, dy = -1; down dy = 1
    dup e-w = if drop else                                      ( x y dx dy )                   \ keep moving in horizontal direction
    dup n-e = if drop 1 = if drop 1 0 else                      ( x y dx dy )                   \ from the north out to east
                 -1 = if 0 -1 else                              ( x y dx dy )                   \ from east out north
                 -1 abort" Invalid n-e entry" then then else    ( x y )
    dup n-w = if drop 1 = if drop -1 0 else                     ( x y dx dy )                   \ from north out west
                 1 = if 0 -1 else                               ( x y dx dy )                   \ from west out north
                 -1 abort" Invalid n-w entry" then then else    ( x y )
    dup s-w = if drop -1 = if drop -1 0 else                    ( x y dx dy )                   \ from south out west
                 1 = if 0 1 else                                ( x y dx dy )                   \ from west out south
                 -1 abort" Invalid s-w entry" then then else    ( x y )
    s-e = if -1 = if drop 1 0 else                              ( x y dx dy )                   \ from south out east
             -1 = if 0 1 else                                   ( x y dx dy )                   \ from east out south
             -1 abort" Invalid s-e entry" then then else        ( x y )
    -1 abort" Invalid segment"
    then then then then then then                               ( x y dx dy )
    2over rot + swap rot + swap                                 ( x y x' y' )
;


: trace-pipe        ( from-x from-y x y -- n)
    0 1 2rot 2rot                                   ( 0 acc fx fy x y )             \ start acc with 1 as we stepped away gtom S to get here
    begin 2dup get-seg [char] S <> while            ( 0 acc fx fy x y )
        in-out                                      ( 0 acc x y next-x next-y)
        2rot 1+ 2rot 2rot                           ( 0 acc' x y next-x next-y )
    repeat                                          ( 0 acc' x y next-x next-y )

    2drop 2drop swap drop                           ( acc )
;


: process           ( -- n )
    cr                                                          (  )
    get-line if buff-len ! store-line else -1 abort" Can't read file" then
    begin get-line while
        buff-len @ <> if -1 abort" Lines not the same length" then
        store-line
    repeat drop

    findS
    2dup ." S = (" swap . ."  , " . ." )" cr                      ( x y )

    2dup find-connection                                         ( x y x' y' )
    2dup ." Found connnection at (" swap . ." , " . ." ) = "
    2dup get-seg emit cr

    trace-pipe                                                  ( num-segs )

;

: .result       ( n -- )
    cr dup . ." steps, so the answer is " 2 / . cr
;

: setup         ( -- )
    pipes q-init
;

: go
    open-input
    setup
    process 
    close-input
    .result
;

