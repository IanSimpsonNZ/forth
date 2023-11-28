
( file open and close - create more 'fd' vars for multiple files)
( usage - s" foo.in" open-input )
0 value fd-in
0 value fd-out
: open-input    ( addr u -- ) r/o open-file throw to fd-in ;
: open-output   ( addr u -- ) w/o create-file throw to fd-out ;
: close-input   ( -- ) fd-in close-file throw ;
: close-output  ( -- ) fd-out close-file throw ;

\ example of line reading routines
\
\ Scan file for a particular line
\
\ 256 Constant max-line
\ Create line-buffer  max-line 2 + allot
\     
\ : scan-file ( addr u -- )
\ begin
\     line-buffer max-line fd-in read-line throw
\ while
\         >r 2dup line-buffer r> compare 0=
\     until
\ else
\     drop
\ then
\ 2drop ;
\
\
\ Copy input to output
\
\ : copy-file ( -- )
\ begin
\     line-buffer max-line fd-in read-line throw
\ while
\     line-buffer swap fd-out write-line throw
\ repeat ;
