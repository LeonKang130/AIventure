import sys
import random
import json
with open('icons_list.json','r') as f:
  icons_list = json.load(f)
# print("获取所有图像url",icons_list) #所有的图像url
# import time
# start_time = time.time()

input = sys.argv[1]
# print(input)

items_num = int(input)#需要返回图片的数量

index_list = random.sample(range(20),items_num)#随机生成0-39整数中items_num个整数
# print(index_list)

generate_list = [] #最终选中的道具描述中文list
for i in range(len(icons_list)):
  for j in index_list:
    if i == j:
      generate_list.append(icons_list[i])

print(generate_list)#打印挑选出的url返回给前端
