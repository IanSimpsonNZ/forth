require ~/forth/libs/files.fs

16 Constant max-line
Create line-buffer  max-line 2 + allot

variable this-sum 0 this-sum !
variable max-sum 0 max-sum !
variable elf-num 1 elf-num !
variable max-elf 0 max-elf !

: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;
: .this-sum     ( -- this-sum-value )   ." Elf #" elf-num ? ." = " this-sum dup @ dup . cr swap 0 swap ! ;
: update-max    ( this-sum-value -- )   dup max-sum @ > if max-sum ! elf-num @ max-elf ! else drop then ;
: next-elf      ( -- )                  1 elf-num +! ;
: blank         ( num-chars -- )        drop .this-sum update-max next-elf ;
: -blank        ( num-chars -- )        get-num this-sum +! ;
: process-line  ( num-chars -- )        dup 0= if blank else -blank then ;
: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: process       ( -- )                  cr begin get-line while process-line repeat blank ( no blank line at eof so process the final set);
: .result       ( -- )                  ." Elf #" max-elf ? ." has " max-sum ? ." calories " ;

: go    s" input.txt" open-input process close-input .result ;