import openai

openai.api_key = "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"

response = openai.Image.create(
  prompt="钢铁做的刀，木制的刀把", #A text description of the desired image(s). The maximum length is 1000 characters.
  n=3, #The number of images to generate. Must be between 1 and 10.
  size="1024x1024" # Must be one of 256x256, 512x512, or 1024x1024
)
for i in response['data']:
  print(i)
