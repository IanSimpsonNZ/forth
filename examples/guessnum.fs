\ A number guessing game
\ 'go' to start a game
\ 'n try' to enter a guess

require ../libs/rand.fs ( for random number generator - choose )

variable answer
variable tries
: go        10 0 do 10 choose drop loop 1000 choose answer ! 0 tries ! ;

: .attempt  tries @ 1+ dup tries ! ." Attempt: " . ;
: .result   answer @ -
            dup 0< if ." Too low " else
            dup 0> if ." Too high " else
            ." Correct! " go then then drop ;
: .too-many tries @ 10 = if cr ." Too many tries, the answer was: " answer ? go then ;

: try       .attempt .result .too-many ;