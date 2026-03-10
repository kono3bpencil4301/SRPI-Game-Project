#!/usr/bin/env python3
"""
图片批量压缩脚本 - 将指定路径的所有图片调整为 512x512 大小
支持格式: JPG, JPEG, PNG, BMP, GIF, WEBP
"""

import os
import sys
from pathlib import Path
from PIL import Image


def resize_image(input_path: str, output_path: str, target_size: tuple = (512, 512)) -> bool:
    """
    将图片调整为指定大小

    Args:
        input_path: 输入图片路径
        output_path: 输出图片路径
        target_size: 目标大小，默认 512x512

    Returns:
        bool: 是否成功处理
    """
    try:
        with Image.open(input_path) as img:
            # 保持图片比例缩放，然后居中裁剪到目标大小
            img.thumbnail((target_size[0] * 2, target_size[1] * 2), Image.Resampling.LANCZOS)

            # 创建目标尺寸的画布
            new_img = Image.new("RGBA", target_size, (0, 0, 0, 0))

            # 计算居中位置
            x = (target_size[0] - img.width) // 2
            y = (target_size[1] - img.height) // 2

            # 如果图片有透明通道，转换为RGBA
            if img.mode in ('RGBA', 'LA') or (img.mode == 'P' and 'transparency' in img.info):
                img = img.convert('RGBA')
            elif img.mode != 'RGBA':
                img = img.convert('RGBA')

            # 粘贴图片到中心
            new_img.paste(img, (x, y), img)

            # 保存
            new_img.save(output_path, 'PNG')
            return True
    except Exception as e:
        print(f"处理图片 {input_path} 时出错: {e}")
        return False


def get_image_files(directory: str) -> list:
    """
    获取目录下的所有图片文件

    Args:
        directory: 目录路径

    Returns:
        list: 图片文件路径列表
    """
    image_extensions = {'.jpg', '.jpeg', '.png', '.bmp', '.gif', '.webp'}
    image_files = []

    directory_path = Path(directory)
    if not directory_path.exists():
        print(f"错误: 目录 {directory} 不存在")
        return []

    for file in directory_path.iterdir():
        if file.is_file() and file.suffix.lower() in image_extensions:
            image_files.append(str(file))

    return image_files


def process_directory(input_dir: str, output_dir: str = None) -> None:
    """
    批量处理目录中的所有图片

    Args:
        input_dir: 输入目录路径
        output_dir: 输出目录路径，如果为None则覆盖原文件
    """
    image_files = get_image_files(input_dir)

    if not image_files:
        print(f"目录 {input_dir} 中没有找到图片文件")
        return

    print(f"找到 {len(image_files)} 张图片")

    # 创建输出目录
    if output_dir:
        os.makedirs(output_dir, exist_ok=True)

    success_count = 0
    for img_path in image_files:
        filename = os.path.basename(img_path)

        if output_dir:
            output_path = os.path.join(output_dir, f"{Path(filename).stem}.png")
        else:
            # 覆盖原文件
            output_path = img_path

        print(f"处理: {filename} ...", end=" ")
        if resize_image(img_path, output_path):
            print("完成")
            success_count += 1
        else:
            print("失败")

    print(f"\n处理完成: {success_count}/{len(image_files)} 张图片成功")


def main():
    if len(sys.argv) < 2:
        print("用法:")
        print("  python image_resizer.py <输入目录> [输出目录]")
        print("\n示例:")
        print("  python image_resizer.py ./images          # 处理images目录，覆盖原文件")
        print("  python image_resizer.py ./images ./output # 处理images目录，输出到output目录")
        sys.exit(1)

    input_directory = sys.argv[1]
    output_directory = sys.argv[2] if len(sys.argv) > 2 else None

    process_directory(input_directory, output_directory)


if __name__ == "__main__":
    main()
