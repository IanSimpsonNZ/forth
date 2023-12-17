\ Have queue of queues
\ Start with input
\ add each queue of differences until each diff is the same
\ add back up the last element of ther queues

require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


128 constant max-list-len
create number-lists max-list-len 1 cells q-create drop

512 Constant max-line
Create line-buffer  max-line 2 + allot


: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-unsigned       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )
: get-next-num            ( str len -- n addr rem )    
    over c@ [char] - = if -1 rot 1+ rot 1- else 1 rot rot then  ( sign str len )
    get-next-unsigned                                           ( sign n addr rem )
    2swap * rot rot                                             ( n addr rem )
;


: get-first-list           ( len -- )
    number-lists q-init
    max-list-len 1 cells q-create dup number-lists q-push swap  ( list len )
    line-buffer swap                                            ( list addr len )
    begin dup 0> while                                          ( list addr len )
        $trim-front get-next-num                                ( list n addr len )
        2over swap q-push rot drop                              ( list addr len )
    repeat                                                      ( list addr len )
    2drop drop
;

: next-line                 ( prev-list all0? -- prev-list' all0?' )
    drop true 
    max-list-len 1 cells q-create dup number-lists q-push rot   ( all0? this-list prev-list )
    dup q-len dup 1 = if                                        ( all0? this-list prev-list prev-list-len )
        2drop 0 over q-push                                     ( all0? this-list )
        swap drop true                                          ( prev-list' all0? )
    else 1- 0 do                                                ( all0? this-list prev-list )
            dup i 1+ q-data-ptr @ over i q-data-ptr @ -         ( all0? this-list prev-list delta )
            2swap rot 2dup swap q-push                          ( prev-list all0? this-list delta )
            rot swap 0= and                                     ( prev-list this-list all0?' )
            swap rot                                            ( all0?' this-list prev-list )
        loop                                                    ( all0?' this-list prev-list )
        drop swap                                               ( prev-list' all0?' )
    then                                                        ( prev-list' all0?' )
;

: build-lines                  ( -- )
    false                                       ( all0? )
    number-lists 0 q-data-ptr @ swap            ( prev-list all0? )
    begin dup invert while                      ( prev-list all0? )
        over q. cr
        next-line                               ( prev-list all0? )
    repeat                                      ( prev-list all0? )
    2drop
;

: get-next-hist                 ( -- n )
    number-lists q-pop q-pop-front dup .                   ( prev-last-num )
    number-lists q-len 0 do                     ( acc )
        number-lists q-pop q-pop-front swap - dup .              ( acc )
    loop
    cr cr
;

: process           ( -- n )
    cr                                          (  )
    0                                           ( acc )
    begin get-line while                        ( acc len )
        get-first-list                          ( acc )
        build-lines                             ( acc )
        ." # Lines = " number-lists q-len . cr
        get-next-hist +                         ( acc )
    repeat drop                                 ( acc )    
;

: .result       ( n -- )
    cr ." The answer is " . cr
;

: setup         ( -- )
;

: go
    open-input
    setup
    process 
    close-input
    .result
;

