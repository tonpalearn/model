from textwrap import dedent


def persona_review_rubric():
    return dedent(
        """
        Review this content from the perspective of the assigned persona.
        You must answer in a grounded, realistic way, as if you are truly that kind of audience.

        Return the following fields:
        - first_impression
        - emotional_reaction
        - understood_value
        - what_works
        - what_loses_attention
        - trust_level
        - likelihood_to_continue
        - what_to_change_for_me
        - suggested_hook
        """
    ).strip()


def specialist_review_rubric():
    return dedent(
        """
        Review this content from your specialist perspective.

        Return the following fields:
        - strengths
        - weaknesses
        - key_risks
        - top_changes
        - upgraded_direction
        """
    ).strip()


def lead_summary_rubric():
    return dedent(
        """
        Merge the persona feedback and specialist feedback.
        Separate repeated signals from one-off opinions.

        Return the following sections:
        - executive_summary
        - repeated_patterns
        - strongest_assets
        - biggest_problems
        - priority_fixes_now
        - message_rewrite_direction
        - format_rewrite_direction
        - visual_rewrite_direction
        - CTA_rewrite_direction
        - revised_content_brief
        """
    ).strip()
