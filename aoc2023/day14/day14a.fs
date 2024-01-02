require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/utils.fs


: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 constant max-line
create line-buffer  max-line 2 + allot
variable buff-len


128 constant max-q
create free-space max-q 2 cells q-create drop
create rolling max-q 2 cells q-create drop

char . constant gap
char O constant round

variable thisfree
variable nextfree
variable thisrolling
variable nextrolling



: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;


: q-dpush           ( dl dh queue -- )                                      \ Pusha  double onto the queue
    dup 2over drop swap q-push                                          ( q dl dh queue )
    dup q-len 1-                                                        ( q dl dh q top-index )
    q-data-ptr 1 cells +                                                ( q dl dh high-addr )
    ! drop                                                              ( q )
;

: dq.            ( queue-addr -- )
    dup dup q-base-pos @ swap q-top-pos @ = if ." Empty " drop else
        dup q-len 0 do
            dup i q-data-ptr d@ d. 
            loop
        drop
    then
;

: process-line      ( -- )
    0 0 0 0 1 0                                                         ( roll-acc-low roll-acc-high free-acc-low free-acc-high bit-value-low bit-value-high )
    buff-len @ 0 do                                                     ( rolll rollh freel freeh bvl bvh )
        line-buffer i + c@                                              ( rolll rollh freel freeh bvl bvh char )
        dup gap = if                                                    ( rolll rollh freel freeh bvl bvh char )
            drop 2swap 2over d+ 2swap                                   ( rolll rollh freel freeh bvl bvh )
        else round = if                                                 ( rolll rollh freel freeh bvl bvh )
            2rot 2over d+ 2rot 2rot                                     ( rolll rollh freel freeh bvl bvh )
        then then                                                       ( rolll rollh freel freeh bvl bvh )
        d2*                                                             ( rolll rollh freel freeh bvl bvh )
    loop                                                                ( rolll rollh freel freeh bvl bvh )
    2drop                                                               ( rolll rollh freel freeh )
    free-space q-dpush                                                   ( rolll rollh )
    rolling q-dpush                                                      (  )
;

: check-len         ( len -- )
    buff-len @ dup 0= if drop buff-len ! else                           (   | len bl )
    <> if -1 abort" Invalid line length" then then                      (  )
;



: roll-line         ( row# -- )
    free-space over q-data-ptr thisfree !                               ( row# )
    free-space over 1+ q-data-ptr nextfree !                            ( row# )
    rolling over q-data-ptr thisrolling !                               ( row# )
    rolling swap 1+ q-data-ptr nextrolling !                            (  )

    thisfree @ d@ nextrolling @ d@                                          ( freel freeh roll rollh )
    rot and rot rot and swap                                            ( roll&gapl roll&gaph )                \ gap in this row and rolling rock in row above

    2dup thisfree @ d@ 2swap d- thisfree @ d!                               ( r&gl r&gh )                   \ fill space in this row
    2dup nextfree @ d@ d+ nextfree @ d!                                     ( r&gl r&gh )                   \ create space in row above]
    2dup thisrolling @ d@ d+ thisrolling @ d!                               ( r&gl r&gh )                   \ adds to rolling rocks on this row
    nextrolling @ d@ 2swap d- nextrolling @ d!                              (  )                            \ moves out of rolling rocks above
;


: roll-down         ( row# -- )                                         \ cascade from row i+1 beack to top
     0 swap do
        i roll-line
    -1 +loop
;

: tilt              ( -- )                                              \ move all the rolling rocks north
    rolling q-len 1- 0 do
        i roll-down
    loop
;

: print-binary      ( d -- )
    begin 2dup d0> while
        2dup drop 2 mod 0= if [char] . emit else [char] x emit then
        d2/
    repeat 2drop
;

: count-binary      ( d -- #1s )
    0 rot rot                                                           ( acc d )
    begin 2dup d0> while                                                ( acc d )
        2dup drop 2 mod 0> if rot 1+ rot rot then                       ( acc d )
        d2/                                                             ( acc d )
    repeat 2drop                                                        ( acc )
;

: print-lines       ( q -- )
    cr
    dup q-len 0 do                                                      ( q )
        dup i q-data-ptr d@                                              ( q d)
        print-binary cr                                                 ( q )
    loop drop
;

: process           ( -- result )
    0 buff-len !
    begin get-line while                                                ( len )
    check-len                                                           (  )                      
    process-line                                                        (  )
    repeat drop                                                         (  )
    tilt

;

: calc-score        ( -- n )
    0                                                                   ( acc )
    rolling q-len 0 do                                                  ( acc )
        rolling i q-data-ptr d@ count-binary                             ( acc )
        rolling q-len i - * +                                           ( acc )
    loop                                                                ( acc )
;

: .result       ( n -- )
    cr ." The answer is " calc-score . cr
;

: setup         ( -- )
    free-space q-init
    rolling q-init
;

: go
    open-input
    setup
    process 
    close-input
    .result
;

\ 53993 too low