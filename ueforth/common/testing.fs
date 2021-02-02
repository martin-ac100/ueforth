( Utilities, probably should go elsewhere )
: mem= ( a a n -- f)
   for aft 2dup c@ swap c@ <> if 2drop rdrop 0 exit then 1+ swap 1+ then next 2drop -1 ;
: str= ( a n a n -- f) >r swap r@ <> if rdrop 2drop 0 exit then r> mem= ;
: startswith? ( a n a n -- f ) >r swap r@ < if rdrop 2drop 0 exit then r> mem= ;
: assert ( f -- ) 0= throw ;

( Support for eval tests )
1000 constant expect-limit
create expect-buffer expect-limit allot
create result-buffer expect-limit allot
variable expect-used   variable result-used
: till;e ( -- n )
   begin >in @ bl parse dup 0= >r s" ;e" str= r> or if exit then drop again ;
: e: ( "name" -- ) create >in @
   till;e over - swap tib + swap dup , $place
   does> dup cell+ swap @ evaluate ;
: expect-emit ( ch -- ) expect-used @ expect-limit < assert
                        expect-buffer expect-used @ + c!
                        1 expect-used +! ;
: result-emit ( ch -- ) result-used @ expect-limit < assert
                        result-buffer result-used @ + c!
                        1 result-used +! ;
: expect-type ( a n -- ) for aft dup c@ expect-emit 1+ then next drop ;
: result-type ( a n -- ) for aft dup c@ result-emit 1+ then next drop ;
: expected ( -- a n ) expect-buffer expect-used @ ;
: resulted ( -- a n ) result-buffer result-used @ ;
: out: ( "line" -- ) nl parse expect-type nl expect-emit ;
: out:\ ( "line" -- ) nl parse expect-type ;
variable confirm-old-type
: confirm{   ['] type >body @ confirm-old-type ! ['] result-type is type ;
: }confirm   confirm-old-type @ is type ;
: expect-reset   0 expect-used ! 0 result-used ! ;
: expect-finish   expected resulted str= if exit then }confirm
   ." Expected:" cr expected type cr ." Resulted:" cr resulted type cr 1 throw ;

( Testing Framework )
( run-tests runs all words starting with "test-", use assert to assert things. )
variable tests-found   variable tests-run    variable tests-passed
: test? ( xt -- f ) >name s" test-" startswith? ;
: for-tests ( xt -- )
   last @ begin dup while dup test? if 2dup >r >r swap execute r> r> then >link repeat 2drop ;
: reset-test-counters   0 tests-found !   0 tests-run !   0 tests-passed ! ;
: count-test ( xt -- ) drop 1 tests-found +! ;
: check-fresh   depth if ."  DEPTH LEAK! " depth . 1 throw then ;
: wrap-test ( xt -- ) expect-reset >r check-fresh r> execute check-fresh expect-finish ;
: red   1 fg ;   : green   2 fg ;   : hr   40 for [char] - emit next cr ;
: run-test ( xt -- ) dup >name type confirm{ ['] wrap-test catch }confirm
   if drop ( cause xt restored on throw ) red ."  FAILED" normal cr
   else green ."  OK" normal cr 1 tests-passed +! then 1 tests-run +! ;
: pre-test-run   cr hr tests-found @ . ." Tests found." cr hr ;
: show-test-results
   hr ."   PASSED: " green tests-passed @ . normal ."   RUN: " tests-run @ .
   ."   FOUND: " tests-found @ . cr
   tests-passed @ tests-found @ = if
     green ."   ALL TESTS PASSED" normal cr
   else
     ."   FAILED: " red tests-run @ tests-passed @ - . normal cr
   then hr ;
: run-tests
   reset-test-counters ['] count-test for-tests pre-test-run
   ['] run-test for-tests show-test-results
   tests-passed @ tests-found @ <> sysexit ;