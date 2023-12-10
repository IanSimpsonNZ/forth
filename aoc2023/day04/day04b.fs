require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: array ( num-elements -- ) create cells allot
    does> ( element-number -- addr ) swap cells + ;


512 Constant max-line
Create line-buffer  max-line 2 + allot

variable buff-len


1024 constant max-multipliers
max-multipliers array multiplier


512 constant max-q-len 
create winning-nums max-q-len 1 cells q-create drop

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )


: $split        ( addr len char -- addr1 len1 addr2 len2 )  \ find delim (char) in string - str1 ends before delim, str2 includes delim
                                                            \ str2 is empty (ie len 0) if delim not found in original string
    rot rot over + over                                 ( char addr end-addr c-addr )
    2swap swap 2swap                                    ( addr char end-addr c-addr )
    begin                                               ( addr char end-addr c-addr )
        2dup >                                          ( addr char end-addr c-addr f )
        2over drop rot dup c@ rot <> rot                ( addr char end-addr c-addr f f )
    and while                                           ( addr char end-addr c-addr )
        1+                                              ( addr char end-addr c-addr )
    repeat                                              ( addr char end-addr c-addr )
    dup rot swap -                                      ( addr char c-addr len2 )
    2swap drop rot 2dup swap -                          ( len2 addr c-addr len1 )
    rot swap 2swap swap                                 ( addr len1 c-addr len2 )
;

: $trim-front   ( addr len -- addr len )                    \ remove leading spaces
    begin 2dup 0> swap c@ 32 = and while                ( addr len )
    1- swap 1+ swap repeat                              ( addr+1 len-1 )
;


: get-winning-nums      ( addr len -- )
    winning-nums q-init
    $trim-front
    begin dup 0> while                              ( addr len )
        $trim-front
        get-next-num rot                            ( addr rem n )
        winning-nums q-push                         ( addr rem )
    repeat
    2drop
;

: match?                ( n -- f )
    false swap                                          ( f n )
    winning-nums q-len 0 do                             ( f n )
        dup winning-nums i q-data-ptr @ = if            ( f n )
            swap drop true swap leave                   ( f n )
        then
    loop drop
;

: check-nums            ( addr len -- n )
    0 rot rot                                           ( #matches addr len )
    $trim-front                                         ( #m addr len )
    begin dup 0> while                                  ( #m addr len )
        $trim-front
        get-next-num 2swap                              ( addr len #m n )
        match? if 1+ then                               ( addr len #m )
        rot rot                                         ( #m addr len )
    repeat                                              ( #m addr len )
    2drop                                               ( #matches )
;

: copy-cards        ( card n -- )                       \ add cards won to multiplier array
    dup 0> if                                           ( card n )
        over multiplier @ 1+ rot 1+ rot                 ( mult+1 card+1 n )
        over + swap                                     ( mult+1 card+n+1 card+1 )
        do dup i multiplier +! loop drop                (  )
    else 2drop then
;


: process-line  ( n -- )                                \ n = card number
    line-buffer buff-len @ [char] : $split              ( card addr1 len1 addr2 len2 )
    2swap 2drop                                         ( card addr2 len2 )
    swap 1+ swap [char] | $split                        ( card addr1 len1 addr2 len2)
    2swap get-winning-nums                              ( card addr2 len2 )
    check-nums                                          ( card n )
    copy-cards                                          (  )
;

: process       ( -- n )
    0                                               ( acc )           \ accumulator
    cr                                              ( acc )
    begin get-line while                            ( acc len )
        line-buffer over type cr
        buff-len !                                  ( acc )
        1+                                          ( acc )         \ assume cards start at one and go up one at a time
        dup process-line                            ( acc )
    repeat drop                                     ( acc )
;

: .result       ( n -- )
    dup 1+ max-multipliers swap do i multiplier @ 0> if
        ." card " i . ."  has " i multiplier @ . cr then loop               \ just checking

    dup 1+ 1 do i multiplier @ + loop                  ( n )

    cr ." The total is " . cr
;

: setup         ( -- )
    0 multiplier max-multipliers cells erase
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
