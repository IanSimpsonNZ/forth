
: animals c" Rat   Ox    Tiger RabbitDragonSnake Horse Ram   MonkeyCock  Dog   Boar  " ;
: .animal ( n -- )
    0 max 11 min
    6 * animals 1+ + 6 type space
;

: juneeshee ( n -- )
    1900 -
    12 mod
    .animal
;