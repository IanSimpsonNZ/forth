require ~/forth/libs/files.fs

256 Constant max-line
Create line-buffer  max-line 2 + allot

variable halfway

: get-halfway   ( n -- )                2 /mod swap if ." Line length not even " -1 abort then halfway ! ;
: score ( c -- score )                  dup emit ."  " dup [char] Z > if [char] a - 1+ else [char] A - 27 + then ;
: check-char    ( c -- score )          0 swap  ( put a found flag on stack before char )
                                        line-buffer halfway @ + dup halfway @ + swap do
                                            dup i c@ = if swap drop -1 swap leave then
                                        loop
                                        swap if score else drop 0 then ;
: find-dup      ( -- score )            0   ( default score )
                                        line-buffer dup halfway @ + swap do 
                                            i c@ check-char
                                            dup if swap drop leave then drop
                                        loop ;
: process-line  ( num-chars -- score )  get-halfway find-dup dup if dup . else ." Nothing " then cr ; 
: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: process       ( -- )                  cr begin get-line while process-line + repeat drop ( do we need to process a blank at the end? ) ;
: .result       ( -- )                  ." Total score is: " . ;
: set-up        ( -- tot-score )        0 ;

: go    set-up s" input.txt" open-input process close-input .result ;