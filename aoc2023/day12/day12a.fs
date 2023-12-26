\ Algorithm - non-recursive
\ 
\ Have the string of . # and ?
\ Have a list with the lengths of broken springs
\ 
\ Mark start of stack -99, -99
\ Put char-pos and num-ptr on stack
\ while not start of stack
\   Can we place here?
\   Yes - 'bump' to next number (next num-ptr char-pos and after the . following the placement)
\   else char-pos + 1
\   If char-pos = len drop char-pos and num-ptr
\ repeat
\
\ place? ( char-pos num-ptr -- char-pos' success? )
\ Are there anough chars in the string? no -> _, false
\ else, do we have ? or # for the number of spaces we need followed by a .
\   Yes - char-pos after . , true
\   No - _, false

\ bump ( char-pos num-ptr -- char-pos' num-ptr' )
\ is num-ptr = len of nums - 1
\ Yes - Add 1 to score, char-pos +1 
\ Else, no - num-ptr + 1


require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/array.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

512 Constant max-line
Create line-buffer  max-line 2 + allot
variable buff-len

max-line $tring springs
char . constant working
char # constant broken
char ? constant unknown

create numbers 128 1 cells q-create drop

variable score

variable loop-count

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;


: store-numbers     ( addr len -- )
    numbers q-init                                              ( addr len )
    begin $trim-front dup 0> while                              ( addr len )
        get-next-unsigned rot numbers q-push                    ( addr len )                   
    repeat 2drop                                                (  )
;

: store-springs     ( addr len )
    springs $init                                               ( addr len )
    2dup springs drop swap move                                 ( addr len )
    springs drop 1 cells -                                      ( addr len springs-len-addr )
    ! drop                                                      (  )
;

: place?            ( c-pos num-ptr -- c-pos' success? )
    2dup ." Trying num-ptr " dup . ." (" numbers swap q-data-ptr ? ." ) at " . cr
    2dup numbers swap q-data-ptr @ dup rot +                    ( c-pos num-ptr num end-spring )
    springs rot <= if 2drop drop false else                     ( c-pos false | c-pos num-ptr num addr )
        2over drop + swap over + dup rot                        ( c-pos num-ptr addr-end addr-end addr-start )
        true rot rot                                            ( c-pos num-ptr addr-end success? addr-end addr-start )
        2dup ." Looping from " . ." to " . cr
        do                                                      ( c-pos num-ptr addr-end success? )
            i c@ emit
            i c@ working = if drop false leave then             ( c-pos num-ptr addr-end success? )
        loop                                                    ( c-pos num-ptr addr-end success? )
        if                                                      ( c-pos num-ptr addr-end )
            dup springs + = if                               ( c-pos num-ptr addr-end+ )             \ end of springs
                ." At end of springs " cr
                2drop drop springs swap drop true               ( c-pos true )
\                springs drop - 1+ true 2swap 2drop              ( c-pos' true )
            else dup c@ dup working = swap unknown = or if                            ( c-pos num-ptr addr-end+ )             \ found a working spring
                dup c@ emit
                ."  Found a working spring " cr
                springs drop - 1+ true 2swap 2drop              ( c-pos' true )
            else                                                ( c-pos num-ptr addr-end+ )
                dup c@ emit
                ."  Fell through " cr
                2drop false                                    ( c-pos false )                         \ neither
            then then                                           ( c-pos success? )
        else 2drop false then                                   ( c-pos false )
    then                                                        ( c-pos success? )
;


: bump              ( c-pos num-ptr -- c-pos' num-ptr' )
    dup numbers q-len 1- = if                                   ( c-pos num-ptr )
        ." No more numbers" cr
        1 score +! 2drop swap 1+ swap                                 ( c-pos' num-ptr )
    else 1+ then                                                ( c-pos num-ptr' )
;


: process-line      (  )
    springs dup 0= if abort" Could not find springs" else type ."  : " then
    numbers dup q-len 0= if abort" Could not find numbers" else q. cr then
    -99 -99                                                     ( -99 -99 )         \ mark start of stack
    0 0                                                         ( c-pos num-ptr )
    0 loop-count !
    begin dup -99 > while                                       ( c-pos num-ptr )
        2dup place?                                             ( c-pos num-ptr c-pos' success? )
        2dup . . cr
        if over bump else drop swap 1+ swap then                ( c-pos num-ptr c-pos' num-ptr' | c-pos' num-ptr )
        over springs swap drop >= if 2drop 2drop then                  ( c-pos num-ptr )
        1 loop-count +!
        loop-count @ 100 > if .s cr -1 abort then
    repeat 2drop                                                (  )
    .s cr
    -1 abort
;

: process           ( -- )
    cr                                                          (  )
    begin get-line while                                        ( len )
        line-buffer swap 32 $split                              ( spring-addr sprint-len numbers-addr numbers-len )
        1- swap 1+ swap                                         ( spring-addr sprint-len numbers-addr numbers-len )  \ move past the space
        store-numbers                                           ( spring-addr sprint-len )
        store-springs                                           (  )
        process-line                                            (  )
    repeat drop
;

: .result       ( n -- )
    cr ." The answer is " score ? cr
;

: setup         ( -- )
    0 score !
;

: go
    open-input
    setup
    process 
    close-input
    .result
;

\ successful places are't moving to next char