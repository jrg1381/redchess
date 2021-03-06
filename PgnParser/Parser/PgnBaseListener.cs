//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Pgn.g4 by ANTLR 4.5.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

namespace Redchess.Pgn {

using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="IPgnListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5.1")]
[System.CLSCompliant(false)]
public partial class PgnBaseListener : IPgnListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.parse"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParse([NotNull] PgnParser.ParseContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.parse"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParse([NotNull] PgnParser.ParseContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.parseTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParseTag([NotNull] PgnParser.ParseTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.parseTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParseTag([NotNull] PgnParser.ParseTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.document"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDocument([NotNull] PgnParser.DocumentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.document"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDocument([NotNull] PgnParser.DocumentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.gameList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterGameList([NotNull] PgnParser.GameListContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.gameList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitGameList([NotNull] PgnParser.GameListContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.generalTagList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterGeneralTagList([NotNull] PgnParser.GeneralTagListContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.generalTagList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitGeneralTagList([NotNull] PgnParser.GeneralTagListContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.game"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterGame([NotNull] PgnParser.GameContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.game"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitGame([NotNull] PgnParser.GameContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.annotation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAnnotation([NotNull] PgnParser.AnnotationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.annotation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAnnotation([NotNull] PgnParser.AnnotationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.annotationList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAnnotationList([NotNull] PgnParser.AnnotationListContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.annotationList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAnnotationList([NotNull] PgnParser.AnnotationListContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.individualMove"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIndividualMove([NotNull] PgnParser.IndividualMoveContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.individualMove"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIndividualMove([NotNull] PgnParser.IndividualMoveContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.moveList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMoveList([NotNull] PgnParser.MoveListContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.moveList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMoveList([NotNull] PgnParser.MoveListContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.move"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMove([NotNull] PgnParser.MoveContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.move"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMove([NotNull] PgnParser.MoveContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.compulsoryTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCompulsoryTag([NotNull] PgnParser.CompulsoryTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.compulsoryTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCompulsoryTag([NotNull] PgnParser.CompulsoryTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.blockComment"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBlockComment([NotNull] PgnParser.BlockCommentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.blockComment"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBlockComment([NotNull] PgnParser.BlockCommentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.enterVariant"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEnterVariant([NotNull] PgnParser.EnterVariantContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.enterVariant"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEnterVariant([NotNull] PgnParser.EnterVariantContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.leaveVariant"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLeaveVariant([NotNull] PgnParser.LeaveVariantContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.leaveVariant"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLeaveVariant([NotNull] PgnParser.LeaveVariantContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.variantLine"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariantLine([NotNull] PgnParser.VariantLineContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.variantLine"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariantLine([NotNull] PgnParser.VariantLineContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.eventTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEventTag([NotNull] PgnParser.EventTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.eventTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEventTag([NotNull] PgnParser.EventTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.siteTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSiteTag([NotNull] PgnParser.SiteTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.siteTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSiteTag([NotNull] PgnParser.SiteTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.dateTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDateTag([NotNull] PgnParser.DateTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.dateTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDateTag([NotNull] PgnParser.DateTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.roundTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRoundTag([NotNull] PgnParser.RoundTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.roundTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRoundTag([NotNull] PgnParser.RoundTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.blackTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBlackTag([NotNull] PgnParser.BlackTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.blackTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBlackTag([NotNull] PgnParser.BlackTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.whiteTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterWhiteTag([NotNull] PgnParser.WhiteTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.whiteTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitWhiteTag([NotNull] PgnParser.WhiteTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.resultTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterResultTag([NotNull] PgnParser.ResultTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.resultTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitResultTag([NotNull] PgnParser.ResultTagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="PgnParser.optionalTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOptionalTag([NotNull] PgnParser.OptionalTagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="PgnParser.optionalTag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOptionalTag([NotNull] PgnParser.OptionalTagContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace Redchess.Pgn
