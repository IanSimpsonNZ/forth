require ~/forth/libs/files.fs

16 Constant max-line
Create line-buffer  max-line 2 + allot


create score
\ Elf move ->   A   B   C
( I lose        Z   X   Y ) 3 , 1 , 2 ,
( I draw        X   Y   Z ) 4 , 5 , 6 ,
( I win         Y   Z   X ) 8 , 9 , 7 ,

: get-score     ( elf me -- score )     [char] X - 3 *
                                        swap [char] A - +
                                        cells score + @ ;
: show-moves ( c1 ptc2 -- c1 ptc2 )     ." My outcome " line-buffer 2 + c@ emit ."  against " line-buffer c@ emit ."  = " ;
: process-line  ( num-chars -- score )  drop show-moves
                                        line-buffer c@ line-buffer 2 + c@ get-score dup . cr ;
: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: process       ( -- )                  cr begin get-line while process-line + repeat drop ( do we need to process a blank at the end? ) ;
: .result       ( -- )                  ." Total score is: " . ;
: set-up        ( -- tot-score )        0 ;

: go    set-up s" input.txt" open-input process close-input .result ;