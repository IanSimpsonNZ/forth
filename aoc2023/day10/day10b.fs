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

variable S-seg

char | constant n-s  \   1 constant _n-s
char - constant e-w  \   2 constant _e-w
char L constant n-e  \   3 constant _n-e
char J constant n-w  \   4 constant _n-w
char 7 constant s-w  \   5 constant _s-w
char F constant s-e  \   6 constant _s-e


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
        pipes i q-data-ptr @
        buff-len @ 0 do
            dup i + c@ dup 128 >= if 128 - emit else emit then
        loop drop cr
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
        dup e-w = swap                               ( f c )
        dup s-w = swap                               ( f f c )
        n-w = or or                                  ( f )
    else 2drop false then                           ( f )
;

: find-connections        ( x y -- dx1 dy1 dx2 dy2 )                          \ Assumes each segment has 2 connections
    0 0 2swap                                           ( 0 acc x y )                                            
    2dup check-up if 2swap 1+ 0 -1 2swap 2rot then      ( 0 acc x y | 0 -1 0 acc' x y )
    2dup check-down if 2swap 1+ 0 1 2swap 2rot then     ( 0 acc x y | 0 1 0 acc' x y )
    2dup check-left if 2swap 1+ -1 0 2swap 2rot then    ( 0 acc x y | -1 0 0 acc' x y )
    check-right if 1+ 1 0 2swap then                    ( 0 acc | 1 0 0 acc )
    2 <> if abort" Num connections <> 2" then
    drop                                                ( dx1 dy1 dx2 dy2 )
;


: get-deltas        ( from-x from-y x y -- x y dx dy )
    2swap 2over rot - swap rot - swap                           ( x y dx dy )
;

: check-deltas      ( dx dy -- )
    2dup 0<> swap 0<> and if -1 abort" Invalid deltas, both non-zero" then
    abs swap abs max 1 > if -1 abort" Invalid delta, > 1" then
;


\ dx =  1 => E
\ dx = -1 => W
\ dy =  1 => S
\ dy = -1 => N
: identify-seg          ( dx1 dy1 dx2 dy2 -- c )
    2dup check-deltas 2over check-deltas
    dup -1 = if                                     ( dx1 dy1 dx2 dy2 | dx1 dy1 dx2 dy2 )        \ N
        2drop                                       ( dx1 dy1 )
        dup 1 = if                                  ( dx1 dy1 )
            2drop n-s                               ( seg )
        else
            0 = if                                  ( dx1 )
                1 = if n-e else n-w then            ( seg )
            else                                    ( dx1 )
                -1 abort" Invalid segment S-S - test 1"
            then                                    ( seg )
        then                                        ( seg )
    else dup 1 = if                                 ( dx1 dy1 dx2 dy2 | dx1 dy1 dx2 dy2 )       \ S
        2drop                                       ( dx1 dy1 )
        dup -1 = if                                 ( dx1 dy1 )
            2drop n-s                               ( seg )
        else
            0 = if                                  ( dx1 )
                1 = if s-e else s-w then            ( seg )
            else                                    ( dx1 )
                -1 abort" Invalid segment N-N - test 2"
            then                                    ( seg )
        then                                        ( seg )
     else swap dup -1 = if                          ( dx1 dy1 dy2 dx2 | dx1 dy1 dy2 dx2 )       \ W
        2drop swap                                  ( dy1 dx1 )
        dup 1 = if                                  ( dy1 dx1 )
            2drop e-w                               ( seg )
        else
            0 = if                                  ( dy1 )
                1 = if s-w else n-w then            ( seg )
            else                                    ( dy1 )
                -1 abort" Invalid segment W-W - test 3"
            then                                    ( seg )
        then                                        ( seg )
    else dup 1 = if                                 ( dx1 dy1 dy2 dx2 | dx1 dy1 dy2 dx2 )       \ E
        2drop swap                                  ( dy1 dx1 )
        dup -1 = if                                 ( dy1 dx1 )
            2drop e-w                               ( seg )
        else
            0 = if                                  ( dy1 )
                1 = if s-e else n-e then            ( seg )
            else                                    ( dy1 )
                -1 abort" Invalid segment E-E - test 4"
            then                                    ( seg )
        then                                        ( seg )
    else -1 abort" Invalid segment"
    then then then then                             ( seg )
;

: find-first-connection   ( x y -- x' y' )
    2dup find-connections                           ( x y dx1 dy1 dx2 dy2 )
    2over 2over identify-seg S-seg c!               ( x y dx1 dy1 dx2 dy2 )
    2drop
    rot + swap rot + swap                           ( y+dy x+dx )

;


: in-out            ( from-x from-y x y -- x y next-x next-y)                       \ given way in and symbol there is only one way out
    2over 2over 2swap swap ." From (" . ." , " . ." ) to (" swap . ." , " . ." )"
    get-deltas 2dup check-deltas
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


: leave-trail       ( x y -- )
    get-c-addr dup c@ 128 + swap c!
;


: trace-pipe        ( from-x from-y x y -- n)
    0 1 2rot 2rot                                   ( 0 acc fx fy x y )             \ start acc with 1 as we stepped away gtom S to get here
    begin 2dup get-seg [char] S <> while            ( 0 acc fx fy x y )
        in-out                                      ( 0 acc x y next-x next-y)
        2rot 1+ 2rot 2dup leave-trail 2rot          ( 0 acc' x y next-x next-y )
    repeat                                          ( 0 acc' x y next-x next-y )
    get-c-addr S-seg @ 128 + swap c!                ( 0 acc x y )                   \ put the "real" S segment in place
    2drop swap drop                                 ( acc )
;

: process-char      ( #crossings acc -- #crossings acc' )
    swap dup 2 mod 1 = if                           ( acc #c )
        swap 1+ else                                ( #c acc' )
        swap then                                   ( #c acc )
;


\ Count number of times you cross the pipe
: process-pipe     ( #c acc c -- #c' acc )
    n-s = if swap 1+ swap then                      ( #c' acc )
;


\ State:
\ 0 - normal
\ 1 - waiting for n-w
\ 2 - waiting for s-w
: pre-scan          ( line-addr -- )                \ clearly mark all n-s crossings - find s-e -> n-w and n-e -> s-w
    0 swap                                          ( state addr )
    dup buff-len @ + swap do                        ( state )
        i c@ dup 128 < if drop else 128 - swap      ( state | c state )
            dup 0= if                               ( c state )
                swap dup s-e = if 2drop 1 else      ( state c | state )             \ Found s-e, wait for n-w
                n-e = if drop 2 then then           ( state )                       \ Either found n-e so wait for s-w, or neither so leave state normal
            else dup 1 = if                         ( c state )
                swap dup n-w = if                   ( state c )                     \ Found n-w, so have a crossing - mark it with n-s and change state
                    drop n-s 128 + i c! drop 0      ( state )
                else s-w = if drop 0 then then      ( state )                       \ Found s-w (ie have F7), so no crossing - back to normal
            else dup 2 = if                         ( c state )
                swap dup s-w = if                   ( state c )                     \ Found s-w, so have a crossing - mark it with n-s and change state
                    drop n-s 128 + i c! drop 0      ( state )
                else n-w = if drop 0 then then      ( state )                       \ Found n-w, (ie have LJ), so no crossing - back to normal
            else -1 abort" Invalid pre-scan state"
            then then then                          ( state )
        then                                        ( state )
    loop drop
;


: scan-line         ( line-addr -- n )
    dup pre-scan                                    ( addr )                    \ Every crossing will be marked with n-s
    0 swap 0 swap                                   ( #crossing acc addr)
    dup buff-len @ + swap do                        ( #c acc )
        i c@                                        ( #c acc c )
        dup 128 < if                                ( #c acc c )
            drop process-char                       ( #c acc )
        else                                        ( #c acc c )
            128 - process-pipe                      ( #c acc )
        then                                        ( #c acc )
    loop                                            ( #c acc )
    swap drop
;

: count-inner       ( -- n )
    0                                               ( acc )
    pipes q-len 0 do
        pipes i q-data-ptr @ scan-line +
    loop
;

: process           ( -- )
    cr                                                          (  )
    get-line if buff-len ! store-line else -1 abort" Can't read file" then
    begin get-line while
        buff-len @ <> if -1 abort" Lines not the same length" then
        store-line
    repeat drop

    findS
    2dup ." S = (" swap . ."  , " . ." )" cr                      ( x y )

    2dup find-first-connection                                    ( x y x' y' )
    2dup ." Found connnection at (" swap . ." , " . ." ) = "
    2dup get-seg emit cr
    ." S is " S-seg c@ emit cr

    trace-pipe drop                                               (  )

    print-pipes
;

: .result       ( n -- )
    cr ." The answer is " count-inner . cr
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
