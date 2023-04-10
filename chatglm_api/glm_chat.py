# -*- coding:utf-8 -*-
import logging
from random import randint
import re
from typing import Callable
from wudao.api_request import getToken
from wudao.utils.sse_util import SSEClient

"""
使用方法：
    1）使用者需要有大模型开放平台(open.bigmodel.cn)的账号。在登录了大模型开放平台后，在上方
       用户手册 -> 新手指南 -> 获取API keys 的页面中，点击 "管理 API Keys"的链接。
       通过 "添加新的 API Key" 的按钮，获取自己的API Key与Public Key，填入下面地两个相应
       变量中（API_KEY，PUBLIC_KEY）。
    2）使用者需要有python3的运行环境，目前本文件开发测试时使用的版本为3.10.4。
    3）使用时需要安装/升级wudao的package。安装/升级命令为：
       pip install --upgrade wudao
       对于国内用户，可以使用腾讯云，目前需要至少更新到1.2.9版本。
       pip3 install --upgrade wudao==1.2.9 -i https://mirrors.cloud.tencent.com/pypi/simple
    在完成以上操作后，就应该能够顺利执行本文件了。

程序介绍
    本程序的实现目标是提供一个对于chatglm api进行能力测试的基础实现。主要演示如何使用相关的API。
    目前版本，本程序支持多轮对话，但每次提问时仅支持单行输入。
    在正常的输入之外，额外提供以下两个命令：
        history     该命令会将当前会话带的所有历史信息全部打出。
        clear       清空多轮对话的历史信息
    如果需要退出本程序，请直接 Ctrl+C。
    
"""

def randomTaskCode():
    return "%019d" % randint(0, 10**19)

MODEL_REQUEST_URL = "https://maas.aminer.cn/api/paas/model/v1/open/engines/sse/chatGLM_ST/chatGLM_ST"

# 接口API KEY
API_KEY = "d68428e024d4445686fa8dd4235d0451"
# 公钥
PUBLIC_KEY = "MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAJPpcl5AXxDvPqiRx0bpZEujhENnNr6uno25keDIzgWTofndVfGyae6jSEUb7htMUxARS/6FZlawNiCKIT3596kCAwEAAQ=="

# 能力类型
ability_type = "chatGLM"
# 引擎类型
engine_type = "chatGLM"

token_result = getToken(API_KEY, PUBLIC_KEY)

_FIELD_SEPARATOR = ":"

def punctuation_converse_auto(msg):
    punkts = [
        [",", "，"],
        ["!", "！"],
        [":", "："],
        [";", "；"],
        ["\?", "？"],
    ]
    for item in punkts:
        msg = re.sub(r"([\u4e00-\u9fff])%s" % item[0], r"\1%s" % item[1], msg)
        msg = re.sub(r"%s([\u4e00-\u9fff])" % item[0], r"%s\1" % item[1], msg)
    return msg

def prepare_print_diff(nextStr: Callable[[any], str], printError: Callable[[], None]):
    previous = ""
    def print_diff(input):
        nonlocal previous
        str = nextStr(input)
        if (not str.startswith(previous)):
            last_line_index = str.rfind("\n") + 1
            if (previous.startswith(str[0: last_line_index])):
                print("\r%s" % str[last_line_index:], end="", flush=True)
            else:
                print()
                print(1, "[[previous][%s]]" % previous)
                printError(input)
        else:
            print(str[len(previous):], end="", flush=True)
        previous = str

    return print_diff

def print_history(history):
    is_request = True
    for history_item in history:
        print("Request:" if is_request else "Response:")
        print("\t", history_item)
        is_request = not is_request

if __name__ == "__main__":
    import requests
    import pprint
    if token_result and token_result["code"] == 200:
        token = token_result["data"]
        headers = {"Authorization": token}

        history = []
        print()
        print("'clear' to clear history and 'history' to show history. Ctrl-C to exit")
        while (True):
            print("Your Input:")
            prompt = input()
            print()
            
            if prompt == "clear":
                history = []
                print("History Cleared.")
                continue
            elif prompt == "history":
                print_history(history)
                print()
                continue

            json = {
                "top_p": 0.7,
                "temperature": 0.5,
                "prompt": prompt,
                "requestTaskNo": randomTaskCode(),
                "history": history,
            }

            response = requests.post(
                MODEL_REQUEST_URL,
                headers=headers,
                json=json,
                stream=True,
            )
            client = SSEClient(response)
            print_diff = prepare_print_diff(lambda e: e.data, lambda e: pprint.pprint(e.__dict__))
            print('Response: ')
            for event in client.events():
                if (event.data):
                    event.data = punctuation_converse_auto(event.data)
                if (event.event == "add"):
                    print_diff(event)
                elif (event.event == "finish" or event.event == "interrupted"):
                    print_diff(event)
                    print()
                    history.extend([prompt, event.data])
                    # print("output_length: \t", len(event.data))
                    break
                elif (event.event == "error"):
                    print_diff(event)
                    print()
                else:
                    pprint.pprint(event.__dict__)
            print()
    else:
        print("获取token失败，请检查 API_KEY 和 PUBLIC_KEY")


