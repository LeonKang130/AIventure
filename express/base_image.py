import openai
import time
import json
openai.api_key = "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"

items_description = ["一把木质的卡通宝剑","一个卡通回血药瓶","一个卡通的紫色的魔法水晶","一个卡通汉堡","一个绿色的卡通灵草","卡通的在山间的泉水","一根卡通的银针线","一个红色的心脏形状的宝石","一个卡通的棕色的陶土罐子","一个卡通的大灵芝",
                     "一个卡通的蓝色的水晶","一个卡通的小精灵","一个卡通的透明的瓶子装满灵丹妙药","一个卡通的女神像","一个红色的诱人的卡通苹果","一个金色的医疗包","一些卡通的水晶碎片","一个卡通的魔法书","一个卡通的天使翅膀","一个卡通的红酒瓶"
                     ]#改成英文哦！一共40个图片
#image
output_url=[]
num=0
for i in items_description:
    print(f"正在生成下表为{num}的图片")
    response = openai.Image.create(
        prompt=i, #A text description of the desired image(s). The maximum length is 1000 characters.
        n=1, #The number of images to generate. Must be between 1 and 10.
        size="256x256" # Must be one of 256x256, 512x512, or 1024x1024
    )
    url = response['data'][0]["url"]
    print("生成完成其url是",url)
    output_url.append(url)
    num+=1
    if num % 5 == 0:#准备冷却5/min
        print("冷却1min")
        time.sleep(60)

#     output_url.append(j.url)
print("base所有的url",output_url)

with open('icons_list.json','w') as f:#将生成的url写入文件
    json.dump(output_url,f)

