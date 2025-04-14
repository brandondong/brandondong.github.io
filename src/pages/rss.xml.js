import rss from '@astrojs/rss';
import { getCollection } from 'astro:content';
import { SITE, BASE } from "../constants";

export async function GET(context) {
    const blog = await getCollection('blog');
    return rss({
        // `<title>` field in output xml
        title: "Brandon's Blog",
        // `<description>` field in output xml
        description: 'Powered by developer procrastination',
        // Pull in your project "site" from the endpoint context
        // https://docs.astro.build/en/reference/api-reference/#site
        site: SITE,
        // Array of `<item>`s in output xml
        // See "Generating items" section for examples using content collections and glob imports
        items: blog.map((post) => ({
            title: post.data.title,
            pubDate: post.data.date,
            // Compute RSS link from post `id`
            // This example assumes all posts are rendered as `/blog/[id]` routes
            link: `/${BASE}/${post.id}/`,
        })),
        // (optional) inject custom xml
        customData: `<language>en-us</language>`,
    });
}