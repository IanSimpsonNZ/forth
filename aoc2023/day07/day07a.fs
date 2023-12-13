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
    1 1                                             ( top next )
    hand-size 1- 0 do                               ( top next )
        1                                           ( top next acc )
        $tmp drop i +                               ( top next acc j-addr )
        dup c@ 32 <> if                             ( top next acc j-addr )
            hand-size i 1+ do                       ( top next acc j-addr )
                $tmp drop i +                       ( top next acc j-addr i-addr )
                2dup c@ swap c@ = if                ( top next acc j-addr i-addr )
                    32 swap c!                      ( top next acc j-addr )
                    swap 1+ swap                    ( top next acc' j-addr )
                else drop then                      ( top next acc j-addr )
            loop                                    ( top next acc j-addr )
        then                                        ( top next acc j-addr )
        drop                                        ( top next acc )
        max 2dup max rot rot min                    ( top' next' )
    loop
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


: hand-swap     ( bubble this b-index t-index -- bubble )
    2over rot hand-list swap q-data-ptr rot swap !      ( bubble this b-index this )    \ bubble in 'this' position
    hand-list rot q-data-ptr !                          ( bubble this )
    drop                                                ( bubble )
;

: card-val          ( char -- n )
    dup dup [char] 0 >= swap [char] 9 <= and if [char] 0 - else ( char | val )
    dup [char] T = if drop 10 else                              ( char | val )
    dup [char] J = if drop 11 else                              ( char | val )
    dup [char] Q = if drop 12 else                              ( char | val )
    dup [char] K = if drop 13 else                              ( char | val )
    dup [char] A = if drop 14 else                              ( char | val )
    -1 abort" Invalid card"
    then then then then then then
;

: compare-cards     ( c1-addr c2-addr -- -1 | 0 | 1 )   \ -1 => c1 < c2; 0 => c1 = c2; 1 => c1 > c2
    c@ card-val swap c@ card-val swap               ( v1 v2 )
    2dup < if 2drop -1 else                         ( v1 v2 | result )
    > if 1 else                                     ( | result )
    0 then then                                     ( result )
;

: firstwins?    ( first second -- f )
    cards swap cards swap                           ( first second )                    \ switch from hand addr to card string addr
    false rot rot                                   ( f first second )
    hand-size 0 do                                  ( f first second )
        2dup compare-cards                          ( f first second res )
        dup 0< if drop leave then                   ( f first second res | f first second )
        0> if rot drop true rot rot leave then      ( f first second )
        1+ swap 1+ swap                             ( f first 'second' )
    loop 2drop
;

: sort-hands    ( -- )                              \ bubble lowest ranked hands to the top
    hand-list q-len 1- 0 do                         (  )
        hand-list i q-data-ptr @                    ( bubble )
        hand-list q-len i 1+ do                     ( bubble )
            hand-list i q-data-ptr @                ( bubble this )
            2dup hand-type c@ swap hand-type c@     ( bubble this this-hand bubble-hand )
            2dup < if 2drop i 1- i hand-swap           ( bubble this this-hand bubble-hand | bubble )
            else = if 2dup firstwins?               ( bubble this | bubble this f )
                if i 1- i hand-swap                    ( bubble this | bubble )
                else swap drop then                 ( this=bubble' )
            else swap drop                          ( this=bubble' )
            then then                               ( bubble )
        loop
        drop
    loop
;

: calc-winnings       ( -- n )
    0
    hand-list q-len 0 do
        hand-list i q-data-ptr @ bet @ i 1+ * +
    loop
;


: process       ( -- n )
    cr
    begin get-line over 0> and while                ( len )
        store-hand                                  ( hand )
        calc-type                                   (  )
    repeat drop

    sort-hands
    print-hands
    calc-winnings
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
