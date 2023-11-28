require ~/forth/libs/structs.fs

0
1 cells field #q-data-len
1 cells field #q-data-item
1 cells field q-base-pos
1 cells field q-top-pos         ( next available cell )
0 field q-data
constant #q-header

: q-init        ( q-addr -- )
    dup 0 swap q-base-pos !
    0 swap q-top-pos !
;

: q-create      ( #q-len #item-size -- addr )              \ usage: q-len item-size q-create queue-ptr !
    2dup *                                  ( q-len i-size q-len-bytes )
    here swap #q-header + allot             ( q-len i-size here )
    dup #q-data-item rot swap !             ( q-len here )
    dup #q-data-len rot swap !              ( here )
    dup q-init                              ( here )
;

: q-info        ( q-addr -- )
    ." Queue address: " dup . cr
    ." Num items at   : " dup #q-data-len .  ."  contains: " dup #q-data-len ? cr
    ." Item len(bytes): " dup #q-data-item . ."  contains: " dup #q-data-item ? cr
    ." Base pointer at: " dup q-base-pos .   ."  contains: " dup q-base-pos ? cr
    ." Top pointer at : " dup q-top-pos  .   ."  contains: " dup q-top-pos ? cr
    ." Data starts at : " #q-header + . cr
;

: q-top-ptr     ( q-ptr -- data-ptr )
    dup                                     ( q-ptr q-ptr )
    q-top-pos @ over #q-data-item @ *       ( q-ptr data-len-bytes )
    swap q-data +                           
;

: q-base-ptr     ( q-ptr -- data-ptr )
    dup
    q-base-pos @ over #q-data-item @ *
    swap q-data +
;

: q-data-ptr    ( q-addr n -- data-ptr )        \ n is the position in the queue relative to q-base-pos
    over q-base-pos @ + over #q-data-len @ mod  \ get the absolute data position
    over #q-data-item @ * swap q-data +         \ convert to pointer
;

: q-len         ( q-addr -- len )
    dup dup q-top-pos @ swap q-base-pos @ - swap #q-data-len @ mod
;

: q.            ( queue-addr -- )
    dup dup q-base-pos @ swap q-top-pos @ = if ." Empty " drop else
        dup q-len 0 do
            dup i q-data-ptr ? 
            loop
        drop
    then
;

: q-check-regs  ( queue-addr -- -1|0 )                      \ true if base pos = top pos
    dup q-top-pos @
    swap q-base-pos @
    =
;

: q-reg-change  ( queue-addr reg-addr delta -- )
    over @ + rot #q-data-len @ mod
    swap !
;

: q-top-incr    ( queue-addr -- )
    dup dup q-top-pos 1 q-reg-change
    q-check-regs if -1 abort" Queue overflow - incr top " then
;

: q-top-decr    ( queue-addr -- )
    dup q-check-regs if -1 abort" Queue underflow - decr top " then
    dup q-top-pos -1 q-reg-change
;

: q-base-incr   ( queue-addr -- )
    dup q-check-regs if -1 abort" Queue underflow - incr base " then 
    dup q-base-pos 1 q-reg-change
;

: q-base-decr    ( queue-addr -- )
    dup dup q-base-pos -1 q-reg-change
    q-check-regs if -1 abort" Queue overflow - decr base " then
;

: q-push        ( n queue-addr -- )
    dup
    q-top-ptr rot swap !                                \ Store value at top of stack
    q-top-incr                                          \ increment top pos (with wraparound) and store
;

: q-pop         ( queue-addr -- n)
    dup
    q-top-decr
    q-top-ptr @
;

: q-push-front  ( n queue-addr -- )
    dup
    q-base-decr
    q-base-ptr !
;

: q-pop-front   ( queue-addr -- n )
    dup
    q-base-ptr @
    swap q-base-incr
;

\ ." Create queue - 512 cells " cr
\ 512 1 cells q-create q1

: q-test        ( queue-addr -- )
    cr dup q-info
    ." push 10 20 30 40 50 " cr
    dup 10 swap q-push
    dup 20 swap q-push
    dup 30 swap q-push
    dup 40 swap q-push
    dup 50 swap q-push
    dup q-info
    .s cr
    ." push 100 200 300 400 500 to front " cr
    dup 100 swap q-push-front
    dup 200 swap q-push-front
    dup 300 swap q-push-front
    dup 400 swap q-push-front
    dup 500 swap q-push-front
    dup q-info
    dup q. cr
    .s cr
    ." pop all elements " cr
    begin dup q-len 0> while dup q-pop-front . repeat
    cr q-info
    .s
;

\ variable 'q1
\ : auto-test
\     512 1 cells q-create 'q1 !
\     'q1 @ q-test
\ ;
