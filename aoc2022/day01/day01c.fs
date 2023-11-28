require ~/forth/libs/files.fs
require ~/forth/libs/time.fs

16 Constant max-line
Create line-buffer  max-line 2 + allot

variable this-sum 0 this-sum !
variable max-sums 2 cells allot
variable elf-num 1 elf-num !
variable max-elves 2 cells allot

( array access )
: max-sum       ( i -- ptr )            cells max-sums + ;
: max-elf       ( i -- ptr )            cells max-elves + ;

( utils )
: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;
: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;

: .this-sum     ( -- )                  ( ." Elf #" elf-num ? ." = " this-sum ? cr ) ;

( store max values )
: thing-swap    ( i xt -- )             dup rot dup 1+ swap rot execute @ swap rot execute ! ;
: shuffle       ( i -- )                dup ['] max-sum thing-swap ['] max-elf thing-swap ;
: store-max     ( i -- )                dup this-sum @ swap max-sum ! elf-num @ swap max-elf ! ;
: insert        ( i -- )                dup dup 2 < if 1 do i shuffle -1 +loop else drop then store-max ;
: check-new-max ( i -- flag )           dup max-sum @ this-sum @ < dup rot rot if insert else drop then ;
: update-max    ( -- )                  3 0 do i check-new-max if leave then loop ;

( line processing )
: next-elf      ( -- )                  1 elf-num +! 0 this-sum ! ;
: blank         ( num-chars -- )        drop .this-sum update-max next-elf ;
: -blank        ( num-chars -- )        get-num this-sum +! ;
: process-line  ( num-chars -- )        dup 0= if blank else -blank then ;

: setup         ( -- )                  max-sums 3 cells erase max-elves 3 cells erase ;
: process       ( -- )                  setup cr begin get-line while process-line repeat blank ( no blank line at eof so process the final set);
: .result       ( -- )                  0 3 0 do ." Elf #" i max-elf ? ." has " i max-sum @ dup . + ." calories " cr loop
                                        ." Total callories for top 3 = " . ;

: go            ( -- )                  s" input.txt" open-input process close-input .result ;
