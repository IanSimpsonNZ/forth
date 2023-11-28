require ~/forth/libs/files.fs

256 Constant max-line
Variable lb1-len Create line-buffer1  max-line 2 + allot
Variable lb2-len Create line-buffer2  max-line 2 + allot
Variable lb3-len Create line-buffer3  max-line 2 + allot


: get-line      ( buff -- n-chr flag )  max-line fd-in read-line throw ;
: score         ( c -- score )          dup [char] Z > if [char] a - 1+ else [char] A - 27 + then ;
: check-char    ( lb lbl c -- flag )    0 swap  ( put a found flag on stack before char )
                                        2swap ( get line buffer ptr and length to top of stack)
                                        over + swap do
                                            dup i c@ = if swap drop -1 swap leave then
                                        loop
                                        drop ;
: find-dup      ( -- score )            0   ( default score )
                                        line-buffer1 dup lb1-len @ + swap do 
                                            line-buffer2 lb2-len @ i c@ check-char
                                            line-buffer3 lb3-len @ i c@ check-char
                                            and
                                            if drop i c@ score leave then
                                        loop ;
: process-line  ( -- score )            find-dup ( dup if dup . else ." Nothing " then cr ) ; 
: get-lines     ( buff -- flag )        line-buffer1 get-line swap lb1-len !
                                        line-buffer2 get-line swap lb2-len !
                                        line-buffer3 get-line swap lb3-len !
                                        and and ;
: process       ( -- )                  cr begin get-lines while process-line + repeat ( do we need to process a blank at the end? ) ;
: .result       ( -- )                  ." Total score is: " . ;
: set-up        ( -- tot-score )        0 ;

: go    set-up s" input.txt" open-input process close-input .result ;