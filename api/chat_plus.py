import openai
#目前还存在很多问题，支持终端输入对话
openai.api_key = "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"

model_engine = "davinci"  # 模型引擎
prompt = "我是黑暗之王，地狱之主，万恶的大魔王。你可别小看我，否则后果自负。"

print(prompt)
while True:
    user_input = input("用户：")  # 接收用户的输入
    if user_input.lower() in ["再见", "退出"]:
        print("再见！")
        break
    
    # 发送用户输入到 OpenAI API 进行对话
    response = openai.Completion.create(
        engine=model_engine,
        prompt=prompt + "\n用户：" + user_input + "\n大魔王：",
        max_tokens=100,
        n=1,
        stop=None,
        temperature=0.7,
    )
    message = response.choices[0].text.strip()
    print("大魔王：" + message)
