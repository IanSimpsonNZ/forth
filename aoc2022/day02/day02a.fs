require ~/forth/libs/files.fs

16 Constant max-line
Create line-buffer  max-line 2 + allot

\ variable this-sum 0 this-sum !

: X ( char1 -- score )                  1 dup . ." + " 
                                        swap c@
                                        dup [char] A = if 3 else
                                        dup [char] B = if 0 else
                                        dup [char] C = if 6 else
                                        ." Invalid move: " emit -1 throw 
                                        then then then swap drop dup . cr
                                        + ;
: Y ( char1 -- score )                  2 dup . ." + "
                                        swap c@
                                        dup [char] A = if 6 else
                                        dup [char] B = if 3 else
                                        dup [char] C = if 0 else
                                        ." Invalid move: " emit -1 throw 
                                        then then then swap drop dup . cr
                                        + ;
: Z ( char1 -- score )                  3 dup . ." + "
                                        swap c@
                                        dup [char] A = if 0 else
                                        dup [char] B = if 6 else
                                        dup [char] C = if 3 else
                                        ." Invalid move: " emit -1 throw 
                                        then then then swap drop dup . cr
                                        + ;
: show-moves ( c1 ptc2 -- c1 ptc2 )     ." I played " line-buffer 2 + @ emit ."  against " line-buffer @ emit ."  = " ;
: process-line  ( num-chars -- c1 c3 )  drop show-moves
                                        line-buffer line-buffer 2 + 1 evaluate ;
: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: process       ( -- )                  cr begin get-line while process-line + repeat drop ( do we need to process a blank at the end? ) ;
: .result       ( -- )                  ." Total score is: " . ;
: set-up        ( -- tot-score )        0 ;

: go    set-up s" input.txt" open-input process close-input .result ;