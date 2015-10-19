grammar Pgn;

options
{
     language=CSharp;
}

import PgnLexer;

@parser::namespace { Redchess.Pgn }

parse : document ;
parseTag : compulsoryTag | optionalTag ;

document : gameList EOF ;

gameList : game+ ;

// Allow the tags to be mixed up even though this isn't what the standard says
generalTagList : (optionalTag|compulsoryTag) (optionalTag|compulsoryTag)* ;

game : generalTagList moveList? GAME_RESULT_END_OF_MOVETEXT? ;

annotation : (blockComment | variantLine) ;
annotationList : annotation annotation* ;
individualMove : foo=(PIECE_TO_SQUARE|CAPTURE|CASTLE_KINGSIDE | CASTLE_QUEENSIDE) (promote=PROMOTES_TO_PIECE)? (checkormate=CHECK|checkormate=MATE)? MOVE_ANALYSIS? (annotation_glyph=NUMERIC_ANNOTATION_GLYPH)? (annotationList)? ;

// Note that black's move is optional because White might have just won!
moveList : move move* ;
// Note that black's move is optional because White might have just won!
move : MOVE_LABEL NO_MOVE? individualMove ((MOVE_LABEL? NO_MOVE)? individualMove)? ;
compulsoryTag : (eventTag | siteTag | dateTag | roundTag | blackTag | whiteTag | resultTag) ;

blockComment : comment=BLOCK_COMMENT ;

enterVariant : LEFT_BRACKET ;
leaveVariant : RIGHT_BRACKET ;
variantLine : enterVariant (moveList | variantLine | blockComment)* leaveVariant ;

eventTag : LEFT_TAG EVENT tag=QUOTED_STRING RIGHT_TAG ;
siteTag : LEFT_TAG SITE tag=QUOTED_STRING RIGHT_TAG ;
dateTag : LEFT_TAG DATE tag=QUOTED_STRING RIGHT_TAG ;
roundTag : LEFT_TAG ROUND tag=QUOTED_STRING RIGHT_TAG ;
blackTag : LEFT_TAG  BLACK tag=QUOTED_STRING RIGHT_TAG ;
whiteTag : LEFT_TAG WHITE tag=QUOTED_STRING RIGHT_TAG ;
resultTag : LEFT_TAG RESULT tag=GAME_RESULT RIGHT_TAG ;
optionalTag : LEFT_TAG tag=TAG_NAME+ tagValue=QUOTED_STRING RIGHT_TAG ;


