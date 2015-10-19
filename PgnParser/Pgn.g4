grammar Pgn;

options
{
     language=CSharp;
}

import PgnLexer;

@header
{
using System.Collections.Generic;
}

@parser::namespace { Redchess.Pgn }

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

parse : document ;
parseTag : compulsoryTag | optionalTag ;

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
		m_processor.ProcessMove(_localctx.foo, _localctx.promote == null ? "" : _localctx.promote.Text.TrimStart(new [] {'='}), _localctx.checkormate == null ? "" : _localctx.checkormate.Text, _localctx.annotation_glyph == null ? "" : _localctx.annotation_glyph.Text);
} ;

// Note that black's move is optional because White might have just won!
moveList : move (WS+ move)* ;
// Note that black's move is optional because White might have just won!
move : MOVE_LABEL WS* (NO_MOVE WS*)? individualMove (WS+ (MOVE_LABEL? WS* NO_MOVE WS*)? individualMove)? ;
compulsoryTag : (eventTag | siteTag | dateTag | roundTag | blackTag | whiteTag | resultTag) ;

blockComment : comment=BLOCK_COMMENT WS*
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
	Event = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

siteTag : LEFT_TAG SITE WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Site = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

dateTag : LEFT_TAG DATE WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Date = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

roundTag : LEFT_TAG ROUND WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Round = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

blackTag : LEFT_TAG  BLACK WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	Black = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

whiteTag : LEFT_TAG WHITE WS+ bar=QUOTED_STRING RIGHT_TAG 
{ 
	White = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\""); 
};

resultTag : LEFT_TAG RESULT WS+ bar=GAME_RESULT RIGHT_TAG
{
	Result = _localctx.bar.Text.Trim(c_doubleQuote);
};

optionalTag : LEFT_TAG foo=TAG_NAME+ WS+ bar=QUOTED_STRING RIGHT_TAG
{
	string trimmedText = _localctx.bar.Text.Trim(c_doubleQuote).Replace("\"\"", "\"");
    m_optionalTags.Add(_localctx.foo.Text, trimmedText); 
	if(_localctx.foo.Text == "FEN")
	{
	    // rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
		m_processor.DoFen(trimmedText);
	}
};

