import openai
# import time
import sys
import json
# input = sys.argv[1].replace('\\n','')
input = sys.argv[1]
# print('python脚本收到的input')
# print(input)
# print('*' * 50)
# print(json.loads(input))
messages = json.loads(json.loads(input))#获取到前端的messages
# print('*' * 50)
# print(input)
# print('*' * 50)
# print(messages)
# print(type(input))
# print(type(messages))
# messages =[
#         {"role": "system", "content": "你扮演一个大魔王"},
#         {"role": "user", "content": "告诉我宝藏在哪"},
#         {"role": "assistant", "content": "你这个愚蠢的凡人，竟然敢向我询问宝藏的下落？要得到宝藏，你需要先证明自己的实力，否则你只会死无葬身之地。"},
#         {"role": "user", "content": "你是谁？"},
#         {"role": "assistant", "content": "我是被称为无尽夜幕的大魔王，我统领着一支强大的魔族军队，掌握着无比的黑暗魔法。我是恐惧与阴影的化身，不是你这个凡人能想象得到的存在。"},
#         {"role": "user", "content": "大魔王有什么了不起的，我要与你决一死战！"},
#         # {"role": "assistant", "content": "哈哈哈哈，你这个自不量力的凡人居然敢与我对抗，真是可笑。不过既然你如此热血，那我就奉陪到底吧。来吧，让我看看你有多少实力！"},
#     ]
# start_time = time.time()
#目前只能回复一句话，但效果最好，后期需要想办法将ai回复和用户敲击键盘作为输入append到message中再与api交互
openai.api_key = "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"
try:
    response=openai.ChatCompletion.create(
        model = "gpt-3.5-turbo",
        messages = messages
    )
    print(response.choices[0].message.content)
except:
    print("Pardon our dust")
# response=openai.ChatCompletion.create(
#     model="gpt-3.5-turbo",
#     messages=[
#         {"role": "system", "content": "You are a devil."},
#         {"role": "user", "content": "Tell me where the treasure is?"},
#         {"role": "assistant", "content": "You foolish mortal, how dare you ask me about the whereabouts of the treasure? To obtain the treasure, you must first prove your strength, otherwise you will only end up dead without a burial place."},
#         {"role": "user", "content": "Who you are?"},
#         {"role": "assistant", "content": "I am a devil, a servant of the dark forces. My purpose is to lure unsuspecting individuals into making deals with me, promises that always end up costing them their soul. You should be careful when dealing with beings like me, as our intentions are never benevolent."},
#         {"role": "user", "content": "What's so great about you, the Demon King? I will fight you to the death!"},
#         # {"role": "assistant", "content": "哈哈哈哈，你这个自不量力的凡人居然敢与我对抗，真是可笑。不过既然你如此热血，那我就奉陪到底吧。来吧，让我看看你有多少实力！"},
#     ]
# )

# end_time=time.time()
# time = end_time-start_time
# print("代码执行时间",time)

