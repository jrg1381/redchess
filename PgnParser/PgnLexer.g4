lexer grammar Pgn;

@lexer::namespace { Redchess.Pgn }

tokens 
{
}

fragment COLUMN : 'a'..'h' ;
fragment ROW : '1'..'8' ;
fragment SQUARE : COLUMN ROW ;
fragment PIECE_SYMBOL : 'K' | 'Q' | 'N' | 'P' | 'R' | 'B' ;
fragment UNAMBIGUOUS_PIECE_WITH_COLUMN : PIECE_SYMBOL COLUMN ;
fragment UNAMBIGUOUS_PIECE_WITH_ROW : PIECE_SYMBOL ROW ;
fragment UNAMBIGUOUS_PIECE_WITH_ROW_AND_COLUMN : PIECE_SYMBOL SQUARE ;

LEFT_TAG : '[' ;
RIGHT_TAG : ']' ;
LEFT_CURLY_BRACKET : '{' ;
RIGHT_CURLY_BRACKET : '}' ;
LEFT_BRACKET : '(' ;
RIGHT_BRACKET : ')' ;
QUOTE : '"' ;
EVENT : 'Event' ;
SITE : 'Site' ;
DATE : 'Date' ;
ROUND : 'Round' ;
WHITE : 'White' ;
BLACK : 'Black' ;
RESULT : 'Result' ;
MATE : '#' ;
CHECK : '+' ;
CASTLE_KINGSIDE : 'O-O' ;
CASTLE_QUEENSIDE : 'O-O-O' ;
PROMOTES_TO : '=' ;
TAKES : 'x' ;

// Recognise whitespace. 
WS : ( ' ' | ( '\r\n' | '\n' ) | '\t' ) -> skip;

// Throw away line comments
LINE_COMMENT : (';' ~['\r\n']*) -> channel(HIDDEN) ;

// These are the only possible results for a game
GAME_RESULT : '"0-1"' | '"1-0"' | '"1/2-1/2"' | '"*"' ;
// The spec says * is not allowed (it's implied) but Shredder writes it out anyway.
GAME_RESULT_END_OF_MOVETEXT : '0-1' | '1-0' | '1/2-1/2' | '*' ;
// The quoted string is used in tag data
QUOTED_STRING : QUOTE ('""' | ~'"')* QUOTE ;
// Looks like e4 or Ra5 or Nba5 or N2a5 or Na1a3 depending on how much disambiguation is needed
PIECE_TO_SQUARE : SQUARE | ((PIECE_SYMBOL | UNAMBIGUOUS_PIECE_WITH_COLUMN | UNAMBIGUOUS_PIECE_WITH_ROW | UNAMBIGUOUS_PIECE_WITH_ROW_AND_COLUMN) SQUARE) ;
// Looks like axe4 or Bxa1
CAPTURE : (COLUMN | PIECE_SYMBOL | UNAMBIGUOUS_PIECE_WITH_COLUMN | UNAMBIGUOUS_PIECE_WITH_ROW | UNAMBIGUOUS_PIECE_WITH_ROW_AND_COLUMN) TAKES SQUARE ;
// Looks like =Q, but we only care about keeping the symbol
PROMOTES_TO_PIECE : PROMOTES_TO PIECE_SYMBOL ;
// A simple integer
INTEGER : '1'..'9' ('0'..'9')* ;
// Integer followed by a mixture of periods and spaces (covers ... for incomplete black moves)
MOVE_LABEL : INTEGER '.' ;
// Keep block comments because they relate to a particular move
BLOCK_COMMENT : LEFT_CURLY_BRACKET (~'}')* RIGHT_CURLY_BRACKET ;
// placeholder move
NO_MOVE : '...'|'..' ;
// User defined tags must look like this
TAG_NAME : ('A'..'Z' | 'a'..'z' | '0'..'9' | '_')+ ;
// Numeric annotation glyph
NUMERIC_ANNOTATION_GLYPH : '$' INTEGER ;
// Move rating annotation
MOVE_ANALYSIS : '?' | '!' | '?!' | '!?' | '!!' | '??' ; // "There are exactly six such annotations"