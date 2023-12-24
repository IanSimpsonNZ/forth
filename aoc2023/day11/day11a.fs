require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/array.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


256 constant edge-size
edge-size edge-size * constant max-stars
create stars max-stars 1 cells q-create drop
edge-size array y-expansion
edge-size array x-expansion
edge-size array x-isblank

512 Constant max-line
Create line-buffer  max-line 2 + allot
variable buff-len

65535 constant x-mask
16 constant y-shift
char # constant star

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;

: create-point      ( x y -- x.y )
    y-shift lshift + 
;

: get-x             ( x.y -- x)
    x-mask and
;

: get-y             ( x.y -- y )
    y-shift rshift
;

: get-xy            ( x.y -- x y )
    dup get-x swap get-y
;

: process-line      ( line-num len -- line-number )
    true rot rot                                                    ( is-blank ln len )
    0 do                                                            ( is-blank ln )
        line-buffer i + c@ star = if                                ( is-blank ln )
            false i x-isblank !                                     ( is-blank ln )
            dup i swap create-point stars q-push                    ( is-blank ln )
            swap drop false swap                                    ( is-blank ln )
        then                                                        ( is-blank ln )
    loop                                                            ( is-blank ln )
    dup 0> if dup 1- y-expansion @ else 0 then                      ( is-blank ln prev )
    rot if 1+ then                                                  ( ln prev )
    over y-expansion !                                              ( ln )
;

: print-stars       (  )
    stars q-len 0 do
        stars i q-data-ptr @ get-xy                             ( x y )
        ." (" swap . ." , " . ." )" cr
    loop
;

: print-y-expansion
    10 0 do
        i y-expansion @ .
    loop cr
;

: print-x-expansion
    10 0 do
        i x-expansion @ .
    loop cr
;


: calc-x-expansion  ( -- )
    0                                                           ( acc )
    edge-size 0 do                                              ( acc )
        i x-isblank @ if 1+ then                                ( acc )
        dup i x-expansion !                                     ( acc )
    loop drop
;

: distance          ( x.y1 x.y2 -- n )
    get-xy                                                      ( x.y1 x2 y2 )
    rot get-xy                                                  ( x2 y2 x1 y1 )
    2over 2over                                                 ( x2 y2 x1 y1 x2 y2 x1 y1 )
    rot y-expansion @ swap y-expansion @ - abs                  ( x2 y2 x1 y1 x2 x1 yexp )
    rot x-expansion @ rot x-expansion @ - abs                   ( x2 y2 x1 y1 yexp xexp )
    2swap 2rot rot - abs swap rot - abs                         ( yexp xexp ydist xdist )
    + + +                                                       ( dist )
;

: dist-test         ( x1 y1 x2 y2 -- )
    create-point rot rot create-point
    distance .
;

: .point            ( x.y -- )
    get-xy ." (" swap . ." , " . ." )"
;

: sum-of-distance   ( -- n )
    0
    stars q-len 1- 0 do                                        ( acc )
        stars i q-data-ptr @                                               ( acc x.y1 )
        stars q-len i 1+ do                                    ( acc x.y1 )
            dup .point ."  -> "
            dup stars i q-data-ptr @
            dup .point ."  = "
            distance                              ( acc x.y1 d )
            dup . cr
            rot + swap                                          ( acc x.y1 )
        loop drop                                               ( acc )
    loop
;

: process           ( -- )
    cr                                                          (  )
    0                                                           ( line-num )
    begin get-line while                                        ( ln len )
        dup buff-len !                                          ( ln )
        process-line                                            ( ln )
        1+                                                      ( ln )
    repeat 2drop                                                (  )
    print-stars                                                 (  )
    calc-x-expansion                                            (  )
    cr print-y-expansion                                        (  )
    cr print-x-expansion cr                                     (  )
;

: .result       ( n -- )
    cr ." The answer is " sum-of-distance . cr
;

: setup         ( -- )
    stars q-init
    0 x-expansion edge-size cells erase
    0 y-expansion edge-size cells erase
    0 x-isblank edge-size cells true fill  \ does this make each 8 byte word false?
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
