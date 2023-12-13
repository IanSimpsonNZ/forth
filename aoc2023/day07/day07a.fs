require ~/forth/libs/files.fs
require ~/forth/libs/strings.fs
require ~/forth/libs/structs.fs
require ~/forth/libs/queue.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


5 constant hand-size

0
hand-size   field cards
1           field hand-type
1 cells     field bet
constant #hand

create hand-list 1024 1 cells q-create drop


512 Constant max-line
Create line-buffer  max-line 2 + allot

5 $tring $tmp


: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )


: store-hand    ( buff-len -- hand-ptr )
    here #hand allot                            ( len hand )
    swap line-buffer swap 32 $split             ( hand cards-addr cards-len bet-chars bet-len )
    $trim-front get-next-num 2drop              ( hand cards-addr cards-len bet )
    2over drop bet !                            ( hand cards-addr cards-len )
    hand-size = if                              ( hand cards-addr )
        over cards hand-size move               ( hand )
        dup hand-list q-push                    ( hand )
    else -1 abort" We don't have the right number of cards?" then
;


: get-matches   ( -- top next )
\    $tmp type cr
    1 1                                             ( top next )
    hand-size 1- 0 do                               ( top next )
        1                                           ( top next acc )
        $tmp drop i +                               ( top next acc j-addr )
        dup c@ 32 <> if                             ( top next acc j-addr )
\            ." Looking for " dup c@ dup . ." =" emit ."  "
            hand-size i 1+ do                       ( top next acc j-addr )
                $tmp drop i +                       ( top next acc j-addr i-addr )
                2dup c@ swap c@ ( 2dup emit ." =" emit ." ?" ) = if                ( top next acc j-addr i-addr )
\                    ." Y  "
                    32 swap c!                       ( top next acc j-addr )
                    swap 1+ swap                    ( top next acc' j-addr )
                else ( ." N  " ) drop then                      ( top next acc j-addr )
            loop                                    ( top next acc j-addr )
        then ( cr )                                      ( top next acc j-addr )
        drop                                        ( top next acc )
        max 2dup max rot rot min                    ( top' next' )
    loop
\    cr $tmp type ." ... " 2dup swap . . cr
;

: get-type      ( top next -- hand-type )
    swap                                            ( next top )
    dup 5 = if 6 else                               ( next top | next top type )
    dup 4 = if 5 else                               ( next top | next top type )
    dup 3 = if swap                                 ( next top | top next )
        dup 2 = if 4 else 3 then                    ( top next type)
        else                                        ( next top )
    dup 2 = if swap                                 ( next top | top next )
        dup 2 = if 2 else 1 then                    ( top next type )
    else 0
    then then then then                             ( next|top top|next type)
    swap drop swap drop                             ( type )
;

: calc-type     ( hand -- )
    dup cards hand-size $tmp $copy                  ( hand )
    get-matches                                     ( hand top next )
    get-type                                        ( hand hand-type )
    swap hand-type c!                               (  )
;

: print-hands   ( -- )
    hand-list q-len 0 do                            (  )
        hand-list i q-data-ptr @                    ( hand )
        dup cards 5 type ."  - "
        dup hand-type c@ . ." - "
        bet @ . cr
    loop
;


: process       ( -- n )
    cr
    begin get-line over 0> and while                ( len )
        store-hand                                  ( hand )
        calc-type                                   (  )
    repeat

    print-hands

    0
;

: .result       ( n -- )
    cr ." The answer is " . cr
;

: setup         ( -- )
   hand-list q-init
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
