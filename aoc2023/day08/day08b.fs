require ~/forth/libs/files.fs
require ~/forth/libs/array.fs
require ~/forth/libs/strings.fs
require ~/forth/libs/queue.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


3 constant node-len
26 26 26 * * constant node-list-len
node-list-len array node-list
16 constant node-bits
1 node-bits lshift constant node-divisor

create paths node-list-len 1 cells q-create drop
create loops node-list-len 1 cells q-create drop

512 Constant max-line
Create line-buffer  max-line 2 + allot

max-line $tring $instructions
node-len $tring $tmp

variable ignoreZ

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )


: get-hash      ( $node -- node# )
    node-len <> if -1 abort" Invalid node name length" then
    0 swap                                              ( node# node-addr )
    dup 3 + swap do                                     ( node# )
        26 * i c@ [char] A - +                          ( node# )
    loop                                                ( node# )
;

: ends-in?              ( char addr len -- f )
    + 1- c@ =
;


: store-node        ( buff-len -- )
    line-buffer swap 32 $split                          ( $node $rest )
    2swap 2dup [char] A rot rot ends-in? rot rot        ( $rest f $node )
    get-hash 2swap                                      ( f node# $rest )
    swap 4 + swap [char] , $split                       ( f node# $left $rest )
    2swap get-hash rot rot                              ( f node# left# $rest )
    swap 2 + swap [char] ) $split 2drop                 ( f node# left# $right )
    get-hash                                            ( f node# left# right# )
    swap node-bits lshift +                             ( f node# lr# )
    over node-list !                                    ( f node# )
    swap if paths q-push else drop then                 (  )
;

: get-left          ( lr# -- left# )
    node-bits rshift
;

: get-right         ( lr# -- right# )
    node-divisor mod
;

: decode-hash       ( hash -- $tmp )
    $tmp over + 1- do
        26 /mod swap [char] A + i c!
    -1 +loop drop
    $tmp
;

: print-nodes            ( -- )
    node-list-len 0 do               (  )
        i node-list @ dup 0<> if                                    ( lr# )
            i decode-hash type ."  = ("                 ( lr# )
            dup get-left decode-hash type ." , "                    ( lr# )
            get-right decode-hash type ." )" cr
        else drop then
    loop
;


: play-game             ( -- num-moves )
    paths q-len 0 do
        paths i q-data-ptr @                                    ( #this )
        0 swap 0 swap                                           ( acc instr-pos #this )
        false ignoreZ !
        1 0 do                                                  ( acc instr-pos #this )
            dup decode-hash type ."  -  "
            rot drop 0 rot rot
            begin [char] Z over decode-hash ends-in? invert ignoreZ @ or while   ( acc instr-pos #this )
                false ignoreZ !
                over $instructions drop + c@                        ( acc instr-pos #this instr )
                rot 1+ $instructions swap drop mod rot rot          ( acc instr-pos' #this instr )  
                dup [char] L = if                                   ( acc instr-pos' #this instr )
                    over node-list @ get-left                       ( acc instr-pos' #this instr #left )
                    rot drop swap                                   ( acc instr-pos' #this' instr )
                else dup [char] R = if                              ( acc instr-pos' #this instr )
                    over node-list @ get-right                      ( acc instr-pos' #this instr #right )
                    rot drop swap                                   ( acc instr-pos' #this' instr )
                else -1 abort" Invalid instruction" then then       ( acc instr-pos' #this instr )
                drop rot 1+ rot rot                                 ( acc' instr-pos' #this )
            repeat                                                  ( acc instr-pos' #this )
            rot dup  . ." Moves -- "                                ( instr-pos' #this acc )
            dup loops q-push
            over ." Ends at " decode-hash type ."  Next moves "     ( instr-pos' #this acc )
            over node-list @ dup get-left decode-hash ." (" type    ( instr-pos' #this acc )
            ." , " get-right decode-hash type ." )" cr              ( instr-pos' #this acc )
            rot rot                                                 ( acc instr-pos' #this )
            true ignoreZ !
        loop cr                                                     ( acc instr-pos' #this )
        2drop drop
    loop
;


: save-instr            ( lb-addr lb-len -- )
    dup rot $instructions drop rot move                 ( lb-len )
    $instructions drop -1 cells + !
;

: print-paths           ( -- )
    paths q-len 0 do
        paths i q-data-ptr @ decode-hash type
        ."  " loops i q-data-ptr @ . cr
    loop
;

: gcd                   ( a b -- gcd )
    begin dup while tuck mod repeat drop
;

: lcm                   ( a b -- lcm )
    over 0= over 0= or if 2drop 0 exit then
    2dup gcd abs */ 
;

: get-loops-lcm         ( -- n )
    loops q-len dup 0= if -1 abort" No loops!" then     ( #loops )
    loops 0 q-data-ptr @ swap                           ( res #loops )
    dup 1 = if drop else                                ( res | res #loops )
        1 do loops i q-data-ptr @ lcm loop              ( res )
    then
;

: process           ( -- n )
    cr
    get-line invert if -1 abort" Could not read instruction line" then
    line-buffer swap save-instr                         (  )
    begin get-line while                                ( len )
        dup 0> if store-node else drop then             (  )
    repeat drop

    cr cr
    play-game

    get-loops-lcm
;

: .result       ( n -- )
    cr ." The answer is " . cr
;

: setup         ( -- )
    $tmp drop 1 cells - 3 swap !
    0 node-list node-list-len -1 fill
    paths q-init
    loops q-init
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
