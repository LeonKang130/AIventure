//对db.js的操作 相关功能封装
const fs = require('fs')
const { promisify } = require('util') //帮助转换promise
const path = require('path')

const readFile = promisify(fs.readFile)//异步读文件
const writeFile = promisify(fs.writeFile)//异步写

const dbPath = path.join(__dirname, './db.json')//动态存储路径

exports.getDb = async () => { //async返回promise
    const data = await readFile(dbPath,'utf8')
    return JSON.parse(data)
}

exports.saveDb = async db => { 
    const data = JSON.stringify(db, null, '  ')//维护db的格式：有回车换行，有缩进null, '  ' 
    await writeFile(dbPath, data)
}
