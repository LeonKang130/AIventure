import openai
openai.api_key = "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"

prompt = """
你好，请你介绍一下chatGPT
"""

response = openai.Completion.create(
              model="text-davinci-003",
              prompt=prompt,
              max_tokens=100,
              temperature=0
            )

print(response['choices'][0]['text'])