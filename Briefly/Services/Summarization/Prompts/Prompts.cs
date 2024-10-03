namespace Briefly.Services.Summarization.Prompts;

public static class Prompts
{
    public const string SummarizeContentPrompt = """
        Summarize the following blog post in 2-5 sentences using simple, direct language.
        Focus on the main idea of the post, and include an important quote from the content if relevant.
        Keep it raw and avoid fancy words or unnecessary details.
        
        The summary should adhere to the following requirements:
        - Be precise, don't mislead and don't be clickbaity.
        - Don't say words that will give the impression you are an LLM - like "delve" for example.
        
        Your output should be the summary itself ONLY
        
        Please summarize the following blog post:\n
        """;

    public const string QuoteFromContentPrompt = """
        Please provide an exact quote from the content that captures the essence of the blog post.
        The quote should be directly copied from the source material without any paraphrasing or modifications,
        and it should consist of at least two sentences.
        If no suitable multi-sentence quote is available, default to a single sentence.

        Make sure:

        - The quote is verbatim from the provided text.
        - Do not create new sentences or rephrase existing content.
        - Only use text that is explicitly written in the original content.
        - Return only the quote itself, with no additional comments or explanations.
        - If no suitable quote is available, respond with "No suitable quote found."\n
        """;
}
