
: bell 7 emit ;

: ring ( -- )
    cr 3 0 do
        ." BEEP" cr bell
        500 ms
    loop 
;