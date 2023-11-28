require ~/forth/libs/files.fs
require ~/forth/libs/structs.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

0
1 cells field name-len
16 field dir-name
1 cells field next-dir
1 cells field parent-dir
1 cells field dir-size
constant #dir-rec

100000 constant max-size
variable size-sum
variable dir-list
variable root-dir

: .dir              ( dir -- )
    cr
    dup ." Address: " . cr
    dup name-len @ dup ." Name len: " . cr          ( dir #name )
    swap dup dir-name rot ." Name: " type cr            ( dir )
    dup dir-size @ ." Size: " . cr
    dup next-dir @ ." Next dir: " . cr
    parent-dir @ ." Parent dir: " . cr
;

: .tree             ( dir -- )
    cr
    begin dup while                             ( dir )
        dup dup dir-name swap name-len @ type       ( dir )
        ." :  " dup dir-size ? cr               ( dir )
        next-dir @                              ( next-dir )
    repeat
    drop
;

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: get-num       ( num-chars -- n )
    0. rot line-buffer swap >number 2drop drop
;

: get-line      ( buff -- n-chr flag )
    line-buffer max-line fd-in read-line throw ;

: create-dir       ( dir name len -- new-dir )
    here #dir-rec allot                 ( dir name len new-dir )        \ create new dir record
    dup name-len 2over swap drop swap ! ( dir name len ndir )           \ store name length
    dup dir-name 2swap swap rot rot         ( dir ndir name nname len )     \ set up str copy
    cmove                               ( dir ndir )                    \ copy name str
    dup parent-dir rot swap !           ( ndir )                        \ set current dir as parent
    dup next-dir 0 swap !               ( ndir )                        \ null next dir
    dup dir-size 0 swap !               ( ndir )                        \ zero dir contents size
;

: add-back          ( dir n -- )
    begin over while                    ( dir n )
        2dup swap dir-size +!           ( dir n )
        swap parent-dir @ swap          ( parent n )
    repeat
    2drop
;

: process-file      ( dir len -- dir )
    get-num                             ( dir n )
    dup 0> if                           ( dir n )
        swap dup rot                    ( dir dir n )
        add-back                        ( dir )
    else drop then                      ( dir )
;

: go-into           ( dir len -- dir )
    5 - line-buffer 5 + swap        ( dir lb+5 len-5 )
    create-dir                      ( new-dir )
    dup next-dir dir-list @ swap !  ( n-dir )
    dup dir-list !                  ( n-dir )
;

: go-up             ( dir -- dir )
    parent-dir @
;

: process-command   ( dir len -- dir )
    line-buffer 2 + c@ [char] c = if        ( dir len )         \ only care about 'cd'
        line-buffer 5 + c@ [char] . = if    ( dir len )
            drop go-up                      ( dir )
        else go-into then               ( dir next-dir)
    else drop then                          ( dir )
;

: process-line      ( dir len -- dir )
    line-buffer c@                      ( dir len c0 )
    [char] $ = if process-command       ( dir )
    else process-file then              ( dir )
;

: get-file-sys     ( dir -- )
    get-line invert if -1 abort" Couldn't read file " then drop              \ first line is always '$ cd /' so throw it away
    begin get-line while                ( dir len )
        process-line                    ( dir )
    repeat
    2drop
;

: calc-dir-size     ( -- n )
    70000000 root-dir @ dir-size @ -                            \ current free space
    30000000 swap -                     ( min )                 \ we need this extra
    root-dir @ dir-size @               ( min ans )
    dir-list @                          ( min ans dir )
    begin dup while                     ( min ans dir )
        dup dir-size @                  ( min ans dir size )
        2over drop >= if                ( min ans dir )
            dup dir-size @ rot min      ( min dir ans )
            swap                        ( min ans dir )
        then                            ( min ans dir )
        next-dir @                      ( min ans dir )
    repeat
    drop swap drop                                
;

: process
    0 s" /" create-dir                  ( root )
    dup dir-list !                      ( root )
    dup root-dir !
    get-file-sys
    dir-list @ .tree
    calc-dir-size
;

: .result       ( -- )
    ." The answer is: " . cr
;

: go
    open-input process .result close-input ;