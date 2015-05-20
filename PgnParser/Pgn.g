grammar Pgn;

options
{
     language=CSharp3;
}

tokens
{
	LEFT_TAG = '[' ;
	RIGHT_TAG = ']' ;
	LEFT_CURLY_BRACKET = '{' ;
	RIGHT_CURLY_BRACKET = '}' ;
	LEFT_BRACKET = '(' ;
	RIGHT_BRACKET = ')' ;
	QUOTE = '"' ;
	EVENT = 'Event' ;
	SITE = 'Site' ;
	DATE = 'Date' ;
	ROUND = 'Round' ;
	WHITE = 'White' ;
	BLACK = 'Black' ;
	RESULT = 'Result' ;
	MATE = '#' ;
	CHECK = '+' ;
	CASTLE_KINGSIDE = 'O-O' ;
	CASTLE_QUEENSIDE = 'O-O-O' ;
	PROMOTES_TO = '=' ;
	TAKES = 'x' ;
}

@lexer::namespace { Redchess.Pgn }
@parser::namespace { Redchess.Pgn }

@header
{
using System;
}

@members
{
	private int m_variantDepth = 0;
	public bool PlayGame;
    private static char[] c_doubleQuote = new [] { '"' }; 

	private Dictionary<string,string> m_optionalTags = new Dictionary<string,string>();
	public IDictionary<string,string> OptionalTags { get { return m_optionalTags; } }

	public string Event { get; private set; }
	public string Site { get; private set; }
	public string Date { get; private set; }
	public string Round { get; private set; }
	public string Black { get; private set; }
	public string White { get; private set; }
	public string Result { get; private set; }
}

fragment COLUMN : 'a'..'h' ;
fragment ROW : '1'..'8' ;
fragment SQUARE : COLUMN ROW ;
fragment PIECE_SYMBOL : 'K' | 'Q' | 'N' | 'P' | 'R' | 'B' ;
fragment UNAMBIGUOUS_PIECE_WITH_COLUMN : PIECE_SYMBOL COLUMN ;
fragment UNAMBIGUOUS_PIECE_WITH_ROW : PIECE_SYMBOL ROW ;
fragment UNAMBIGUOUS_PIECE_WITH_ROW_AND_COLUMN : PIECE_SYMBOL SQUARE ;

// Recognise whitespace. 
WS : ( ' ' | ( '\r\n' | '\n' ) | '\t' ) ;

// Throw away line comments
LINE_COMMENT : (';' . '\r\n') { $channel = Hidden; } ;

// These are the only possible results for a game
GAME_RESULT : '"0-1"' | '"1-0"' | '"1/2-1/2"' | '"*"' ;
// The spec says * is not allowed (it's implied) but Shredder writes it out anyway.
GAME_RESULT_END_OF_MOVETEXT : '0-1' | '1-0' | '1/2-1/2' | '*' ;
// The quoted string is used in tag data
QUOTED_STRING : QUOTE ('""' | ~QUOTE)* QUOTE ;
// Looks like e4 or Ra5 or Nba5 or N2a5 or Na1a3 depending on how much disambiguation is needed
PIECE_TO_SQUARE : SQUARE | ((PIECE_SYMBOL | UNAMBIGUOUS_PIECE_WITH_COLUMN | UNAMBIGUOUS_PIECE_WITH_ROW | UNAMBIGUOUS_PIECE_WITH_ROW_AND_COLUMN) SQUARE) ;
// Looks like axe4 or Bxa1
CAPTURE : (COLUMN | PIECE_SYMBOL | UNAMBIGUOUS_PIECE_WITH_COLUMN | UNAMBIGUOUS_PIECE_WITH_ROW | UNAMBIGUOUS_PIECE_WITH_ROW_AND_COLUMN) TAKES SQUARE ;
// Looks like =Q, but we only care about keeping the symbol
PROMOTES_TO_PIECE : PROMOTES_TO symbol=PIECE_SYMBOL { Text = symbol.Text; };
// A simple integer
INTEGER : '1'..'9' ('0'..'9')* ;
// Integer followed by a mixture of periods and spaces (covers ... for incomplete black moves)
MOVE_LABEL : INTEGER '.' ;
// Keep block comments because they relate to a particular move
BLOCK_COMMENT : LEFT_CURLY_BRACKET (~RIGHT_CURLY_BRACKET)* RIGHT_CURLY_BRACKET ;
// placeholder move
NO_MOVE : '...'|'..' ;
// User defined tags must look like this
TAG_NAME : ('A'..'Z' | 'a'..'z' | '0'..'9' | '_')+ ;
// Numeric annotation glyph
NUMERIC_ANNOTATION_GLYPH : '$' INTEGER ;
// Move rating annotation
MOVE_ANALYSIS : '?' | '!' | '?!' | '!?' | '!!' | '??' ; // "There are exactly six such annotations"

public parse : document ;
public parseTag : compulsoryTag | optionalTag ;

document : gameList EOF ;

gameList : game+ ;

// Allow the tags to be mixed up even though this isn't what the standard says
generalTagList : (optionalTag|compulsoryTag) (WS* (optionalTag|compulsoryTag))* ;

game : generalTagList WS+ (moveList)? WS* GAME_RESULT_END_OF_MOVETEXT? WS*
{
	if(PlayGame)
	{
		m_processor.ResetGame();
		m_optionalTags.Clear();
	}
};

annotation : (blockComment | variantLine) ;
annotationList : annotation (WS+ annotation)* ;
individualMove : foo=(PIECE_TO_SQUARE|CAPTURE|CASTLE_KINGSIDE | CASTLE_QUEENSIDE ) (promote=PROMOTES_TO_PIECE)? (checkormate=CHECK|checkormate=MATE)? MOVE_ANALYSIS? (WS+ annotation_glyph=NUMERIC_ANNOTATION_GLYPH)? (WS+ annotationList)?
{
	if(PlayGame && m_variantDepth == 0)
		m_processor.ProcessMove(foo, promote == null ? "" : promote.Text, checkormate == null ? "" : checkormate.Text, annotation_glyph == null ? "" : annotation_glyph.Text);
} ;

// Note that black's move is optional because White might have just won!
moveList : move (WS+ move)* ;
// Note that black's move is optional because White might have just won!
move : MOVE_LABEL WS* (NO_MOVE WS*)? individualMove (WS+ (MOVE_LABEL? WS* NO_MOVE WS*)? individualMove)? ;
compulsoryTag : (eventTag | siteTag | dateTag | roundTag | blackTag | whiteTag | resultTag) ;

blockComment : comment=BLOCK_COMMENT
{
	// Console.WriteLine(comment.Text);
};

enterVariant : LEFT_BRACKET
{
	m_variantDepth++;
};

leaveVariant : RIGHT_BRACKET
{
	m_variantDepth--;
};

variantLine : enterVariant (moveList | variantLine | blockComment)* WS* leaveVariant
{
	// Console.WriteLine(actual_text.Text);
};

eventTag : LEFT_TAG EVENT WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Event = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

siteTag : LEFT_TAG SITE WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Site = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

dateTag : LEFT_TAG DATE WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Date = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

roundTag : LEFT_TAG ROUND WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Round = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

blackTag : LEFT_TAG  BLACK WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Black = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

whiteTag : LEFT_TAG WHITE WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	White = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

resultTag : LEFT_TAG RESULT WS+ bar=GAME_RESULT RIGHT_TAG
{
	Result = bar.Text.Trim(c_doubleQuote);
};

optionalTag : LEFT_TAG foo=TAG_NAME+ WS+ bar=QUOTED_STRING RIGHT_TAG
{
	string trimmedText = bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\"");
    m_optionalTags.Add(foo.Text, trimmedText); 
	if(foo.Text == "FEN")
	{
	    // rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
		m_processor.DoFen(trimmedText);
	}
};

