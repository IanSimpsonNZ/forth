require ~/forth/libs/files.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: get-num       ( num-chars -- n )
    0. rot line-buffer swap >number 2drop drop
;

: get-line      ( buff -- n-chr flag )
    line-buffer max-line fd-in read-line throw ;


: process
;

: .result       ( -- )
    ." The answer is: " . cr
;

: go
    open-input process .result close-input ;