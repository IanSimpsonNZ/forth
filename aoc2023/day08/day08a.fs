require ~/forth/libs/files.fs
require ~/forth/libs/array.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


3 constant node-len
26 26 26 * * constant node-list-len
node-list-len array node-list
16 constant node-bits
1 node-bits lshift constant node-divisor

512 Constant max-line
Create line-buffer  max-line 2 + allot

max-line $tring $instructions
node-len $tring $tmp

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

: store-node        ( buff-len -- )
    line-buffer swap 32 $split                          ( $node $rest )
    2swap get-hash rot rot                              ( node# $rest )
    swap 4 + swap [char] , $split                       ( node# $left $rest )
    2swap get-hash rot rot                              ( node# left# $rest )
    swap 2 + swap [char] ) $split 2drop                 ( node# left# $right )
    get-hash                                            ( node# left# right# )
    swap node-bits lshift +                             ( node# lr# )
    swap node-list !                                    (  )
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

: play-game             ( target# -- num-moves )
    0 swap 0 swap 0                                         ( acc instr-pos #target #this )
    begin 2dup <> while                                     ( acc instr-pos #target #this )
        rot dup $instructions drop + c@                     ( acc #target #this instr-pos instr )
        swap 1+ $instructions swap drop mod swap            ( acc #target #this instr-pos' instr )
        2swap rot                                           ( acc instr-pos' #target #this instr )
        dup [char] L = if drop node-list @ get-left         ( acc instr-pos' #target #this instr | acc instr-pos' #target #left )
        else dup [char] R = if drop node-list @ get-right   ( acc instr-pos' #target #this instr | acc instr-pos' #target #left )
        else -1 abort" Invalid instruction" then then       ( acc instr-pos' #target #left )
        2swap swap 1+ swap 2swap                            ( acc instr-pos' #target #left )
    repeat                                                  ( acc instr-pos #target #this )
    2drop drop
;


: save-instr            ( lb-addr lb-len -- )
    dup rot $instructions drop rot move                 ( lb-len )
    $instructions drop -1 cells + !
;

: process           ( -- n )
    cr
    get-line invert if -1 abort" Could not read instruction line" then
    line-buffer swap save-instr                         (  )
    begin get-line while                                ( len )
        dup 0> if store-node else drop then             (  )
    repeat drop

    $instructions type cr
    print-nodes

    25 26 26 * * 25 26 * + 25 + play-game                         \ search for ZZZ
;

: .result       ( n -- )
    cr ." The answer is " . cr
;

: setup         ( -- )
    $tmp drop 1 cells - 3 swap !
    0 node-list node-list-len erase
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
