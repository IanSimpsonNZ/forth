require ~/forth/libs/files.fs

256 Constant max-line
Create line-buffer  max-line 2 + allot

: get-line      ( buff -- n-chr flag )  line-buffer max-line fd-in read-line throw ;

: get-num       ( str len -- n addr rem )   0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-nums      ( len -- n1 n2 n3 n4 )      line-buffer swap
                                            4 0 do get-num loop
                                            2drop ;

: contains?     ( s1 e1 s2 e2 -- 1|0 )  rot - ( end 2 - end 1 ) ( s1 s2 e2-e1 )
                                        swap rot - ( s2-s1 )
                                        * 0> if 0 else 1 then ;

( if s1 in s2e2 or s2 in s1e1 return 1 else 0 )
: intersects?   ( s1 e1 s2 e2 -- 1|0 )  2over 2over     ( s1 e1 s2 e2 s1 e1 s2 e2 )
                                        rot 2drop       ( s1 e1 s2 e2 s1 s2 )
                                        swap 2swap      ( s1 e1 s2 s1 s2 e2 )
                                        rot dup         ( s1 e1 s2 s2 e2 s1 s1 )
                                        contains?       ( s1 e1 s2 1|0 )
                                        swap 2swap      ( 1|0 s2 s1 e1 )
                                        rot dup         ( 1|0 s1 e1 s2 s2 )
                                        contains?       ( 1|0 1|0 )
                                        or ;

: process-line  ( len -- score )        get-nums intersects? ; 
: process       ( -- )                  cr begin get-line while process-line + repeat drop ( do we need to process a blank at the end? ) ;
: .result       ( -- )                  ." Total score is: " . ;
: set-up        ( -- tot-score )        0 ;

: go    set-up s" input.txt" open-input process close-input .result ;