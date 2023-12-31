require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs


: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

256 constant max-line
create line-buffer  max-line 2 + allot
variable buff-len


64 constant max-q
create col-nums max-q 1 cells q-create drop
create row-nums max-q 1 cells q-create drop

char # constant rock

variable #po2


: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;


: process-line      ( col-bit-value -- line-value )
    1 0                                                                 ( col-bit-value row-bit-value acc )
    buff-len @ 0 do                                                     ( cbv rbv acc )
        line-buffer i + c@ rock = if                                    ( cbv rbv acc )
            over +                                                      ( cbv rbv acc )
            rot dup col-nums i q-data-ptr +! rot rot                    ( cbv rbv acc )
        then                                                            ( cbv rbv acc )
        swap 2 * swap                                                   ( cbv rbv acc )
    loop                                                                ( cbv rbv acc )
    swap drop swap drop
;

: init-col-nums     ( len -- )
    dup buff-len !
    col-nums q-init
    0 do 0 col-nums q-push loop
;

: calc-ids          ( -- more-blocks? )
    row-nums q-init                                                     (  )
    1 0                                                                 ( col-bit-value row-num )
    begin get-line 2dup swap 0> and while                               ( cbv row-num len flag )
        drop swap dup 0= if swap init-col-nums else swap drop then 1+   ( cbv row-num )       ( rn )
        swap dup process-line row-nums q-push 2 * swap                  ( cbv row-num )
    repeat                                                              ( cbv row-num )
    2swap 2drop swap drop                                               ( flag )
;

: powerof2         ( n1 -- f )                                       \ true if number is a single power of 2
    dup 1- and 0=                                                       ( result )
    dup if 1 #po2 +! then                                               ( result )
;

: eq-or-po2         ( n1 n2 -- f )                                      \ true if numbers are equal or only differ by a single power of 2
    xor dup 0= if drop true else powerof2 then                          ( flag )    \ need to test for zro (ie exactly the same) so we don't increment #po2
;

: check-reflection  ( num-q pos -- found-it? )
    0 #po2 !
    dup 0< if 2drop true else                                           ( true | num-q pos )  \ DELETED THE =
        true swap dup                                                   ( num-q found-it? pos pos )
        1+ 0 do                                                         ( num-q found-it? pos )         \ pos has already been checked to idetify reflexction candidate
            rot                                                         ( found-it? pos num-q )
            over i + 1+                                                 ( found-it? pos num-q other-pos )
            over q-len over <= if drop rot rot leave then               ( found-it? pos num-q other-pos | num-q found-it? pos )
            over swap q-data-ptr @                                      ( found-it? pos num-q other-num )
            swap rot 2dup i - q-data-ptr @                              ( found-it? other-num num-q pos num )
            2swap swap rot eq-or-po2 invert if rot drop false rot leave then          ( found-it? pos num-q | num-q found-it? pos )
            rot rot                                                     ( num-q found-it? pos )
        loop                                                            ( num-q found-it? pos )
        drop swap drop                                                  ( found-it? )
    then                                                                ( found-it? )
    #po2 @ 1 = and
;

: find-reflections  ( nums-q -- num-to-left-or-above )
    0 swap                                                              ( res q )
    dup q-len 1- 0 do                                                   ( res q )
        dup i q-data-ptr @                                              ( res q ith )
        over i 1+ q-data-ptr @                                          ( res q ith i+1th) \ Can't just add 1 to address because queues are circular
        eq-or-po2 if dup i check-reflection else false then                     ( res q found-it? )
        dup if ." Found reflection at " i 1+ . then
        if swap drop i 1+ swap leave then                               ( res q )
    loop drop                                                           ( res )
;

: process-block     ( -- cols-to-left rows-above more-blocks? )
    calc-ids                                                            ( more-blocks? )
    cr ." Check cols - "
    col-nums find-reflections swap                                      ( #cols more-blocks? )
    cr ." Check rows - "
    row-nums find-reflections swap                                      ( #cols #rows more-blocks? )
    cr .s cr
;

: process-block-results     ( col-sum row-sum cols rows -- col-sum row-sum )
    100 * rot + swap rot + swap                                 ( col-sum row-sum )
;

: process           ( -- result )
    0 0                                                         ( col-sum row-sum )
    begin process-block while                                   ( col-sum row-sum cols rows )
    process-block-results                                       ( col-sum row-sum )
    repeat process-block-results                                ( col-sum row-sum )
    +                                                           ( result )
;

: .result       ( n -- )
    cr ." The answer is " . cr
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

