import openai
import time
start_time = time.time()
openai.api_key = "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"

#translation
chinese_text='很多蝴蝶在粉色的花丛中飞舞'#接受用户输入的中文字符串
translation = openai.ChatCompletion.create(
  model="gpt-3.5-turbo",
  messages=[
    {"role": "user", "content": f'Translate the following chinese text to Engilish: "{chinese_text}"'}
  ]
)

print(translation.choices[0].message.content)
English_text = translation.choices[0].message.content

#image
response = openai.Image.create(
  prompt=English_text, #A text description of the desired image(s). The maximum length is 1000 characters.
  n=1, #The number of images to generate. Must be between 1 and 10.
  size="256x256" # Must be one of 256x256, 512x512, or 1024x1024
)
end_time=time.time()
time = end_time-start_time
print("代码执行时间",time)
for i in response['data']:
  print(i)#图片url
