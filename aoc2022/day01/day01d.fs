require ~/forth/libs/files.fs
require ~/forth/libs/time.fs

16 Constant max-line
Create line-buffer  max-line 2 + allot

variable this-sum 0 this-sum !

( utils )
: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;
: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;

( store top 3 )
\ Assume for big list most will be outside top 3
\ So have 1st 2nd 3rd on stack to allow easy access
\ to 3rd.
: check-1st     ( t1 t2 t3 -- t1 t2 t3 )    rot rot 2dup ( t2 t1 n t1 n )
                                            < if swap ( t2 n t1 ) then ( t2 t1 n ) rot ( n t1 t2 or t1 n t2 );
: check-2nd     ( t1 t2 t3 -- t1 t2 t3 )    2dup < if swap ( t1 n t2 ) check-1st then ;
: podium        ( t1 t2 t3 -- t1 t2 t3 )    this-sum @ 2dup ( t1 t2 t3 n t3 n )
                                            < if swap drop  ( t1 t2 n) check-2nd else drop then ;

( line processing )
: next-elf      ( -- )                      0 this-sum ! ;
: blank         ( t1 t2 t3 nc -- t1 t2 t3)  drop podium next-elf ;
: -blank        ( num-chars -- )            get-num this-sum +! ;
: process-line  ( t1 t2 t3 nc -- t1 t2 t3)  dup 0= if blank else -blank then ;

: setup         ( -- 0 0 0 )            0 0 0 ;
: process       ( -- t1 t2 t3 )         setup cr begin get-line while process-line repeat blank ( no blank line at eof so process the final set);
: .result       ( t1 t2 t3 -- )         ." Total callories for top 3 = " + + . ;

: go            ( -- )                  s" input.txt" open-input process close-input .result ;


